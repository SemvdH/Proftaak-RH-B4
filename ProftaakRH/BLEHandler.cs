using System;
using System.Collections.Generic;
using System.Text;
using Avans.TI.BLE;
using System.Threading;
using System.Security.Cryptography;

namespace Hardware
{
    class BLEHandler
    {
        IDataConverter dataConverter;
        private BLE bleBike;
        private BLE bleHeart;
        public bool Running { get; set; }

        public BLEHandler(IDataConverter dataConverter)
        {
            this.dataConverter = dataConverter;
            bool running = false;
        }

        public void Connect()
        {
            BLE bleBike = new BLE();
            Thread.Sleep(1000); // We need some time to list available devices

            // List available devices
            List<String> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine(name);
                if (name.Contains("Avans Bike"))
                {
                    Console.WriteLine("connecting to {0}", name);
                    Connect(name);
                    break;

                }
            }
        }
        public async void Connect(string deviceName)
        {
            int errorCode = 0;
            bleBike = new BLE();
            bleHeart = new BLE();

            errorCode = errorCode = await bleBike.OpenDevice(deviceName);
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            // Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            // Subscribe
            bleBike.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            // Heart rate
            errorCode = await bleHeart.OpenDevice(deviceName);
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            errorCode = await bleHeart.SetService("HeartRate");
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            bleHeart.SubscriptionValueChanged += BleBike_SubscriptionValueChanged;
            errorCode = await bleHeart.SubscribeToCharacteristic("HeartRateMeasurement");
            if (errorCode > 0)
            {
                disposeBLE();
                return;
            }

            Console.WriteLine("connected to BLE");
            this.Running = true;
        }

        private void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            //Console.WriteLine("Received from {0}: {1}", e.ServiceName,
            //    BitConverter.ToString(e.Data).Replace("-", " "));
            //send to dataconverter

            if (e.ServiceName == "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e")
            {
                byte[] payload = new byte[8];
                Array.Copy(e.Data, 4, payload, 0, 8);
                this.dataConverter.Bike(payload);
            }
            else if (e.ServiceName == "00002a37-0000-1000-8000-00805f9b34fb")
            {
                this.dataConverter.BPM(e.Data);
            }
            else
            {
                Console.WriteLine("received data from unknown source {0}", e.ServiceName);
            }

        }

        private void disposeBLE()
        {
            this.bleBike?.Dispose();
            this.bleHeart?.Dispose();
            this.Running = false;
        }

        public void setResistance(float percentage)
        {
            byte[] antMessage = new byte[13];
            antMessage[0] = 0x4A;
            antMessage[1] = 0x09;
            antMessage[2] = 0x4E;
            antMessage[3] = 0x05;
            antMessage[4] = 0x30;
            for (int i = 5; i < 11; i++)
            {
                antMessage[i] = 0xFF;
            }
            antMessage[11] = (byte)Math.Max(Math.Min(Math.Round(percentage / 0.5), 255), 0);
            //antMessage[11] = 50; //hardcoded for testing

            byte checksum = 0;
            for (int i = 0; i < 12; i++)
            {
                checksum ^= antMessage[i];
            }

            antMessage[12] = checksum;//reminder that i am dumb :P


            bleBike.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", antMessage);
        }
    }
}
