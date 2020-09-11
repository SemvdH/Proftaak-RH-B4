using LibNoise.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Hardware.Simulators
{
    class BikeSimulator
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

        byte[] array;



        public BikeSimulator(IDataConverter dataConverter)
        {
            this.dataConverter = dataConverter;
        }
        public void StartSimulation()
        {
            //4A-09-4E-05-19-16-00-FF-28-00-00-20-F0

            float x = 0.0f;

            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0,LibNoise.NoiseQuality.Best);
            
            while (true)
            {
                CalculateVariables(improvedPerlin.GetValue(x)+1);
                dataConverter.Bike(GenerateBike0x19());
                dataConverter.Bike(GenerateBike0x10());
                dataConverter.BPM(GenerateHeart());

                Thread.Sleep(1000);
                x += 0.1f;
                eventCounter++;
                elapsedTime++;
            }

        }
        private byte[] GenerateBike0x19()
        {
            byte[] bikeByte = { 0x19, Convert.ToByte(eventCounter%256), 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            return bikeByte;
        }

        private byte[] GenerateBike0x10()
        {
            string binary = Convert.ToString((int)speed, 2);
            byte b = Convert.ToByte(binary.Substring(0, 3));
            byte b2 = Convert.ToByte(binary.Substring(8));
            byte[] bikeByte = { 0x10, Convert.ToByte(equipmentType), Convert.ToByte(elapsedTime*4%64), Convert.ToByte(distanceTraveled), b, b2, Convert.ToByte(BPM), 0xFF };
            return bikeByte;
        }

        private byte[] GenerateHeart()
        {
            byte[] hartByte = { 0x00, Convert.ToByte(BPM)};
            return hartByte;
        }

        private void CalculateVariables(float perlin)
        {
            this.speed = perlin * 5 / 0.001 ;
            array = BitConverter.GetBytes(speed);
            
            Console.WriteLine(speed);
            this.distanceTraveled = (distanceTraveled+speed) % 256;
            this.BPM = (int) perlin * 80;
            this.cadence = (int)speed * 4;

        }

        /*private double Random(double x)
        {
            return (Math.Sin(2 * x) + Math.Sin(Math.PI * x) + 2) / 4;
        }*/
    }
}
