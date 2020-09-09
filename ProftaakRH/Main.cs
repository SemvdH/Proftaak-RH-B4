using System;
using System.Collections.Generic;
using System.Text;
using Hardware;
using Hardware.Simulators;

namespace ProftaakRH
{
    class Program 
    {
        static void Main(string[] agrs)
        {
            IDataConverter dataConverter = new DataConverter();
            BikeSimulator bikeSimulator = new BikeSimulator(dataConverter);
            bikeSimulator.StartSimulation();

        }
    }
}
