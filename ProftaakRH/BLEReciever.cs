using System;
using System.Collections.Generic;
using System.Text;
using Avans.TI.BLE;
using System.Threading;


namespace Hardware
{
    class BLEReciever
    {
        IDataConverter dataConverter;
        private BLE bleBike;
        private BLE bleHeart;
        public bool running { get; }

        public BLEReciever(IDataConverter dataConverter)
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
            this.running = true;
        }

        private void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine("Received from {0}: {1}", e.ServiceName,
                BitConverter.ToString(e.Data).Replace("-", " "));
            //send to dataconverter
        }

        private void disposeBLE()
        {
            this.bleBike?.Dispose();
            this.bleHeart?.Dispose();
            this.running = false;
        }
    }
}
