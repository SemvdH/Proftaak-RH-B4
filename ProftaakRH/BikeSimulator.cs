using LibNoise.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Hardware.Simulators
{
    class BikeSimulator : IHandler
    {
        IDataConverter dataConverter;
        private int elapsedTime = 0;
        private int eventCounter = 0;
        private double distanceTraveled = 0;
        private int equipmentType = 25;
        private double speed = 0;
        private int BPM = 0;
        private int cadence = 0;
        private double resistance = 0;

        //Array for the speed bytes
        byte[] array;



        public BikeSimulator(IDataConverter dataConverter)
        {
            this.dataConverter = dataConverter;
        }
        public void StartSimulation()
        {
            //Example BLE Message
            //4A-09-4E-05-19-16-00-FF-28-00-00-20-F0

            float x = 0.0f;

            //Perlin for Random values
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0,LibNoise.NoiseQuality.Best);
            
            while (true)
            {
                CalculateVariables(improvedPerlin.GetValue(x)+1);
                
                Console.WriteLine("BikeSimulation:\nSpeed: " + this.speed / 100 + "m/s\t" + this.BPM + "BPM\n"+"Resis: "+ this.resistance+"%\n");

                //Simulate sending data
                dataConverter.Bike(GenerateBike0x19());
                dataConverter.Bike(GenerateBike0x10());
                dataConverter.BPM(GenerateHeart());

                

                Thread.Sleep(1000);

                x += 0.1f;
                eventCounter++;
                elapsedTime++;
            }

        }

        //Generate an ANT message for page 0x19
        private byte[] GenerateBike0x19()
        {
            byte[] bikeByte = { 0x19, Convert.ToByte(eventCounter%256), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            return bikeByte;
        }

        //Generate an ANT message for page 0x10
        private byte[] GenerateBike0x10()
        {
            byte[] bikeByte = { 0x10, Convert.ToByte(equipmentType), Convert.ToByte(elapsedTime*4%64), Convert.ToByte(distanceTraveled), array[0], array[1], Convert.ToByte(BPM), 0xFF };
            return bikeByte;
        }

        //Generate an ANT message for BPM
        private byte[] GenerateHeart()
        {
            byte[] hartByte = { 0x00, Convert.ToByte(BPM)};
            return hartByte;
        }

        //Generate an ANT message for resistance
        public byte[] GenerateResistance(float percentage)
        {
            byte[] antMessage = new byte[13];
            antMessage[0] = 0x4A;
            antMessage[1] = 0x09;
            antMessage[2] = 0x4E;
            antMessage[3] = 0x05;
            antMessage[4] = 0x30;
            for (int i = 5; i < 11; i++)
            {
                antMessage[i] = 0xFF;
            }
            antMessage[11] = (byte)Math.Max(Math.Min(Math.Round(percentage / 0.5), 255), 0);
            //antMessage[11] = 50; //hardcoded for testing

            byte checksum = 0;
            for (int i = 0; i < 12; i++)
            {
                checksum ^= antMessage[i];
            }

            antMessage[12] = checksum;//reminder that i am dumb :P

            return antMessage;
        }

        //Calculates the needed variables
        //Input perlin value
        private void CalculateVariables(float perlin)
        {
            this.speed = perlin * 5 / 0.01 ;
            short sped = (short)speed;
            array = BitConverter.GetBytes(sped);
            this.distanceTraveled = (distanceTraveled+(speed*0.01)) % 256;
            this.BPM = (int) (perlin * 80);
            this.cadence = (int)speed * 4;

        }

        //Set resistance in simulated bike
        public void setResistance(byte[] bytes)
        {
            //TODO check if message is correct
            if(bytes.Length == 13)
            {
                this.resistance = Convert.ToDouble(bytes[11])/2;                
            }
        }
    }

    //Interface for receiving a message on the simulated bike
    interface IHandler
    {
        void setResistance(byte[] bytes);

    }
}
