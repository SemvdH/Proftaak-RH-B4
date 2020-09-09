using LibNoise.Primitive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;



namespace Hardware.Simulators
{
    class BikeSimulator
    {
        IDataConverter dataConverter;
        private int eventCounter = 0;

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
                byte[] array = { 0x19, 0x16, 0x00, 0xFF, 0x28, 0x00, 0x00, 0x20 };
                Console.WriteLine(improvedPerlin.GetValue(x)+1);
                //0x10 message
                /*foreach(byte s in array)
                {
                    Console.Write("{0:X}",s);
                }*/
                dataConverter.Bike(GenerateBike(improvedPerlin.GetValue(x)+1));
                //0x19 message
                //dataConverter.Bike(array);
                //Heartbeat message
                //dataConverter.BPM(array);

                Thread.Sleep(1000);
                x += 1f;
                eventCounter++;
            }

        }
        private byte[] GenerateBike(float perlin)
        {
            byte[] bikeByte = {0x19,0x}
            return new byte[1];
        }

        private double Random(double x)
        {
            return (Math.Sin(2 * x) + Math.Sin(Math.PI * x) + 2) / 4;
        }
    }
}
