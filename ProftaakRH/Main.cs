using System;
using System.Collections.Generic;
using System.Text;
using Hardware;
using System.Threading;


namespace ProftaakRH
{
    class Program
    {

        static void Main(string[] args)
        {
            IDataConverter dataConverter = new DataConverter();
            BLEHandler bLEHandler = new BLEHandler(dataConverter);

            bLEHandler.Connect();

            while (!bLEHandler.Running)
            {
                Thread.Yield();
            }

            bLEHandler.setResistance(25);
            bool running = true;
            while (running)
            {
                string input = Console.ReadLine();
                input.ToLower();
                input.Trim();
                if(input == "quit")
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
