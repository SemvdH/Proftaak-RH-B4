using System;
using Hardware;
using Hardware.Simulators;
using RH_Engine;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //connect fiets?

            Thread.Sleep(20000);
            Client client = new Client();


            while (!client.IsConnected())
            {

            }
            BLEHandler bLEHandler = new BLEHandler(client);

            bLEHandler.Connect();

            client.setHandler(bLEHandler);


            //BikeSimulator bikeSimulator = new BikeSimulator(client);

            //bikeSimulator.StartSimulation();

            //client.setHandler(bikeSimulator);

            while (true)
            {
            }
        }
    }
}
