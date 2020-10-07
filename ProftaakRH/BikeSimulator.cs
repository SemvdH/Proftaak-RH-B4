using LibNoise.Primitive;
using ProftaakRH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Hardware.Simulators
{
    public class BikeSimulator : IHandler
    {
        List<IDataReceiver> dataReceivers;
        private int elapsedTime = 0;
        private int eventCounter = 0;
        private double distanceTraveled = 0;
        private int equipmentType = 25;
        private double speed = 0;
        private int BPM = 0;
        private int cadence = 0;
        private double resistance = 0;
        private double power;
        private double accPower;

        byte[] speedArray;
        byte[] powerArray;
        byte[] accPowerArray;



        public BikeSimulator(IDataReceiver dataReceiver)
        {
            this.dataReceivers = new List<IDataReceiver> { dataReceiver };
        }

        public BikeSimulator(List<IDataReceiver> dataReceivers)
        {
            this.dataReceivers = dataReceivers;
        }

        public void addDataReceiver(IDataReceiver dataReceiver)
        {
            this.dataReceivers.Add(dataReceiver);
        }

        public void StartSimulation()
        {
            Console.WriteLine("simulating bike...");
            //Example BLE Message
            //4A-09-4E-05-19-16-00-FF-28-00-00-20-F0

            float x = 0.0f;

            //Perlin for Random values
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0, LibNoise.NoiseQuality.Best);

            while (true)
            {
                CalculateVariables(improvedPerlin.GetValue(x) + 1);

                //Simulate sending data
                foreach (IDataReceiver dataReceiver in this.dataReceivers)
                {
                    dataReceiver.Bike(GenerateBike0x19());
                    dataReceiver.Bike(GenerateBike0x10());
                    dataReceiver.BPM(GenerateHeart());
                }

                Thread.Sleep(1000);

                x += 0.1f;
                eventCounter++;
                elapsedTime++;
            }

        }

        //Generate an ANT message for page 0x19
        private byte[] GenerateBike0x19()
        {
            byte statByte = (byte)(powerArray[1] >> 4);
            byte[] bikeByte = { 0x19, Convert.ToByte(eventCounter % 256), Convert.ToByte(cadence % 254), accPowerArray[0], accPowerArray[1], powerArray[0], statByte, 0x20 };
            return bikeByte;
        }

        //Generate an ANT message for page 0x10
        private byte[] GenerateBike0x10()
        {
            byte[] bikeByte = { 0x10, Convert.ToByte(equipmentType), Convert.ToByte(elapsedTime * 4 % 64), Convert.ToByte(distanceTraveled), speedArray[0], speedArray[1], Convert.ToByte(BPM), 0xFF };
            return bikeByte;
        }

        //Generate an ANT message for BPM
        private byte[] GenerateHeart()
        {
            byte[] hartByte = { 0x00, Convert.ToByte(BPM) };
            return hartByte;
        }



        //Calculates the needed variables
        //Input perlin value
        private void CalculateVariables(float perlin)
        {
            this.speed = perlin * 5 / 0.01;
            short sped = (short)speed;
            speedArray = BitConverter.GetBytes(sped);
            this.distanceTraveled = (distanceTraveled + (speed * 0.01)) % 256;
            this.BPM = (int)(perlin * 80);
            this.cadence = (int)speed / 6;
            this.power = ((1 + resistance) * speed) / 14 % 4094;
            this.accPower = (this.accPower + this.power) % 65536;
            // TO DO power to power LSB & MSN
            powerArray = BitConverter.GetBytes((short)this.power);
            accPowerArray = BitConverter.GetBytes((short)accPower);
        }

        //Set resistance in simulated bike
        public void setResistance(float percentage)
        {
            this.resistance = (byte)Math.Max(Math.Min(Math.Round(percentage / 0.5), 255), 0);
        }

    }

}
