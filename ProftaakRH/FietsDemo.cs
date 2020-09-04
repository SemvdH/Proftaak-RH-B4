using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FietsDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            int errorCode = 0;
            BLE bleBike = new BLE();
            BLE bleHeart = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<String> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }

            // Connecting
            errorCode = errorCode = await bleBike.OpenDevice("Avans Bike B5F0");
            // __TODO__ Error check

            var services = bleBike.GetServices;
            foreach (var service in services)
            {
                Console.WriteLine($"Service: {service}");
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            // __TODO__ error check

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

            // Heart rate
            errorCode = await bleHeart.OpenDevice("Avans Bike B5F0");

            await bleHeart.SetService("HeartRate");

            bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");

            Console.Read();
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine("Received from {0}: {1}, {2}", e.ServiceName,
                BitConverter.ToString(e.Data).Replace("-", " "),
                Encoding.UTF8.GetString(e.Data));

            string[] bytes = BitConverter.ToString(e.Data).Split('-');
            string[] ANT = new string[5];
            if (e.ServiceName == "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e")
            {
                Console.WriteLine("SYNC     : " + bytes[0]);
                ANT[0] = bytes[0];
                Console.WriteLine("LENGTH   : " + bytes[1]);
                
                int length = Convert.ToInt32(bytes[1], 16);
                ANT[1] = length.ToString();
                Console.WriteLine("MSG ID   : " + bytes[2]);
                ANT[2] = bytes[2];
                string msg = string.Empty;
                for (int i = 3; i < 3 + length; i++)
                {
                    msg += bytes[i];
                }
                ANT[3] = msg;

                Console.WriteLine("MSG      : " + msg);
                string checksum = bytes[3 + length];
                ANT[4] = checksum;
                Console.WriteLine("CHECKSUM : " + checksum);
            } else
            {
                Console.WriteLine("BPM:     " + Convert.ToInt32(bytes[1], 16));
            }
            Console.WriteLine();
        }
    }
}