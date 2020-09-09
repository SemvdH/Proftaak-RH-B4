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
            errorCode = errorCode = await bleBike.OpenDevice("Avans Bike AC48");
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
            errorCode = await bleHeart.OpenDevice("Avans Bike AC48");

            await bleHeart.SetService("HeartRate");

            bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");

            Console.Read();
        }

        private static void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            //Console.WriteLine("Received from {0}: {1}, {2}", e.ServiceName,
            //    BitConverter.ToString(e.Data).Replace("-", " "),
            //    Encoding.UTF8.GetString(e.Data));

            string[] bytes = BitConverter.ToString(e.Data).Split('-');
            string[] ANT = new string[5];
            if (e.ServiceName == "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e")
            {
                //Console.WriteLine("SYNC     : " + bytes[0]);
                //ANT[0] = bytes[0];
                //Console.WriteLine("LENGTH   : " + bytes[1]);

                int length = Convert.ToInt32(bytes[1], 16);
                //ANT[1] = length.ToString();
                //Console.WriteLine("MSG ID   : " + bytes[2]);
                //ANT[2] = bytes[2];
                //string msg = string.Empty;
                //for (int i = 3; i < 3 + length; i++)
                //{
                //    msg += bytes[i];
                //}
                //ANT[3] = msg;

                byte[] message = new byte[length - 1];

                Array.Copy(e.Data, 4, message, 0, length - 1);

                DoCrazyShitWithMsg(message);

                //Console.WriteLine("MSG      : " + msg);
                //string checksum = bytes[3 + length];
                //ANT[4] = checksum;
                //Console.WriteLine("CHECKSUM : " + checksum);

                //byte calcChecksum = 0;

                //for (int i = 0; i < e.Data.Length - 1; i++)
                //{
                //    calcChecksum ^= e.Data[i];
                //}

                //Console.WriteLine("Calculated checksum : " + Convert.ToString(calcChecksum,16).ToUpper());


                //Console.WriteLine(BitConverter.ToString(e.Data));

            }
            else
            {
                //Console.WriteLine("BPM:     " + Convert.ToInt32(bytes[1], 16));
            }
            Console.WriteLine();
        }

        private static void DoCrazyShitWithMsg(byte[] bytes)
        {
            Console.WriteLine("doing crazy shit with {0}", bytes);
            String[] hexvalues = BitConverter.ToString(bytes).Split('-');
            for (int i = 0; i < hexvalues.Length; i++)
            {
                Console.WriteLine("Byte {0}: {1}", i, hexvalues[i]);
            }
            switch (bytes[0])
            {
                case 0x10:
                    Console.WriteLine();
                    Console.WriteLine("Data Page Number\t\t: " + hexvalues[0]);
                    Console.WriteLine("Equipment Type Bit Field\t: " + hexvalues[1]);
                    Console.WriteLine("Elapsed Time\t\t\t: " + hexvalues[2]);
                    Console.WriteLine("Distance Traveled\t\t: " + hexvalues[3]);
                    Console.WriteLine("Speed LSB\t\t\t: " + hexvalues[4]);
                    Console.WriteLine("Speed LSB\t\t\t: " + hexvalues[5]);
                    Console.WriteLine("Heart Rate\t\t\t: " + hexvalues[6]);
                    Console.WriteLine("Capabilities FE State Bit Field\t: " + Convert.ToString(bytes[7], 2));
                    break;
                case 0x19:
                    Console.WriteLine();
                    Console.WriteLine("Data Page Number\t\t: " + hexvalues[0]);
                    Console.WriteLine("Update Event Count\t\t: " + hexvalues[1]);
                    Console.WriteLine("Instantaneous Cadence\t\t: " + hexvalues[2]);
                    Console.WriteLine("Accumulated Power LSB\t\t: " + hexvalues[3]);
                    Console.WriteLine("Accumulated Power MSB\t\t: " + hexvalues[4]);
                    Console.WriteLine("Instant Power LSB\t\t: " + hexvalues[5]);
                    Console.WriteLine("Instant Power MSN and status\t: " + Convert.ToString(bytes[6], 2));
                    Console.WriteLine("Flags and FE state BitField\t: " + Convert.ToString(bytes[7], 2));
                    break;
                default:
                    Console.WriteLine("data page number not recognized");
                    break;


            }


            //Console.WriteLine("Speed might be {0} m/s", bytes[3] * 0.093294);



        }
    }
}