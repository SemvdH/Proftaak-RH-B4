using System;
using System.Collections.Generic;
using System.Text;
using Hardware;
using ProftaakRH;

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
            while (true)
            {
            }
        }
    }
}
