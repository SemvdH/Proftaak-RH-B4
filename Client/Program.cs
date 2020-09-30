using System;
using Hardware;
using Hardware.Simulators;
using RH_Engine;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //connect fiets?


            Client client = new Client();


            while (!client.IsConnected())
            {

            }
            //BLEHandler bLEHandler = new BLEHandler(client);

            //bLEHandler.Connect();

            BikeSimulator bikeSimulator = new BikeSimulator(client);

            bikeSimulator.StartSimulation();

            while (true)
            {
            }
        }
    }
}
