﻿using System;
using Hardware;
using Hardware.Simulators;

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
