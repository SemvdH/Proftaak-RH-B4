using System;
using System.Collections.Generic;
using System.Text;
using Hardware;


namespace ProftaakRH
{
    class Program 
    {
        static void Main(string[] agrs)
        {
            IDataConverter dataConverter = new DataConverter();
            BikeSimulator bikeSimulator = new BikeSimulator(dataConverter);
            bikeSimulator.StartSimulation();

            bLEReceiver.Connect();

            Console.ReadLine();
        }
    }
}
