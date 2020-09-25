using System;
using System.Collections.Generic;
using System.Text;
using Avans.TI.BLE;
using System.Threading;
using System.Security.Cryptography;
using ProftaakRH;

namespace Hardware
{
    /// <summary>
    /// <c>BLEHandler</c> class that handles connection and traffic to and from the bike
    /// </summary>
    public class BLEHandler
    {
        List<IDataReceiver> dataReceivers;
        private BLE bleBike;
        private BLE bleHeart;
        public bool Running { get; set; }

        /// <summary>
        /// Makes a new BLEHandler object
        /// </summary>
        /// <param name="dataReceiver">the dataconverter object</param>
        public BLEHandler(IDataReceiver dataReceiver)
        {
            this.dataReceivers = new List<IDataReceiver> { dataReceiver };
        }

        public BLEHandler(List<IDataReceiver> dataReceivers)
        {
            this.dataReceivers = dataReceivers;
        }

        public void addDataReceiver(IDataReceiver dataReceiver)
        {
            this.dataReceivers.Add(dataReceiver);
        }

        /// <summary>
        /// Checks for available devices to connect to, and if one is found, it connects to it
        /// </summary>
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

        /// <summary>
        /// Connects to the device with the given name
        /// </summary>
        /// <param name="deviceName">The name of the device to connect to</param>
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

        /// <summary>
        /// Callback for when the subscription value of the ble bike has changed
        /// </summary>
        /// <param name="sender"> the sender object</param>
        /// <param name="e">the value changed event</param>
        private void BleBike_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {

            if (e.ServiceName == "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e")
            {
                byte[] payload = new byte[8];
                Array.Copy(e.Data, 4, payload, 0, 8);
                foreach (IDataReceiver dataReceiver in this.dataReceivers)
                {
                    dataReceiver.Bike(payload);
                }
            }
            else if (e.ServiceName == "00002a37-0000-1000-8000-00805f9b34fb")
            {
                foreach (IDataReceiver dataReceiver in this.dataReceivers)
                {
                    dataReceiver.BPM(e.Data);
                }
            }
            else
            {
                Console.WriteLine("received data from unknown source {0}", e.ServiceName);
            }

        }

        /// <summary>
        /// Disposes of the current BLE object, if it exists.
        /// </summary>
        private void disposeBLE()
        {
            this.bleBike?.Dispose();
            this.bleHeart?.Dispose();
            this.Running = false;
        }

        /// <summary>
        /// Method <c>setResistance</c> converts the input percentage to bytes and sends it to the bike.
        /// </summary>
        /// <param name="percentage">The precentage of resistance to set</param>
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
