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
            Console.ReadLine();
        }
    }
}
