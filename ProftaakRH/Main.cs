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
            IDataReceiver dataReceiver = new DataConverter();
            BLEHandler bLEHandler = new BLEHandler(dataReceiver);
            //BikeSimulator bikeSimulator = new BikeSimulator(dataConverter);
            //bikeSimulator.setResistance(bikeSimulator.GenerateResistance(1f));
            //bikeSimulator.StartSimulation();


            bool running = true;
            while (running)
            {
                string input = Console.ReadLine();
                input.ToLower();
                input.Trim();
                if (input == "quit")
                {
                    running = false;
                    break;
                }
                try
                {
                    int resistance = Int32.Parse(input);
                    bLEHandler.setResistance(resistance);
                }
                catch
                {
                    //do nothing
                }
            }
        }
    }
}
