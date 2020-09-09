using System;
using System.Collections.Generic;
using System.Text;
using Hardware;

namespace ProftaakRH
{
    class Program
    {

        static void Main(string[] args)
        {
            IDataConverter dataConverter = new DataConverter();
            BLEReciever bLEReceiver = new BLEReciever(dataConverter);

            bLEReceiver.Connect("sdkgjnzsoifgnzaiof");

            //Console.ReadLine();
        }
    }
}
