using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hardware.Simulators
{
    class BikeSimulator
    {
        IDataConverter dataConverter;

        public BikeSimulator(IDataConverter dataConverter)
        {
            this.dataConverter = dataConverter;
        }
        public void StartSimulation()
        {
            //4A-09-4E-05-19-16-00-FF-28-00-00-20-F0
            
            while (true)
            {
                byte[] array = { 0x19, 0x16, 0x00, 0xFF, 0x28, 0x00, 0x00, 0x20, 0xF0 };
                
                //0x10 message
                dataConverter.Bike(array);
                //0x19 message
                dataConverter.Bike(array);
                //Heartbeat message
                dataConverter.BPM(array);

                Thread.Sleep(1000);

            }

        }
    }
}
