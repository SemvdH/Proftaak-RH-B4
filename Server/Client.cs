using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;

namespace Server
{
    class Client
    {
        private Communication communication;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private byte[] totalBuffer = new byte[1024];
        private int bytesReceived;


        public string Username { get; set; }

        public Client(Communication communication, TcpClient tcpClient)
        {
            this.communication = communication;
            this.tcpClient = tcpClient;
            this.stream = this.tcpClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void OnRead(IAsyncResult ar)
        {
            int receivedBytes = this.stream.EndRead(ar);
            byte[] lengthBytes = new byte[4];

            Array.Copy(this.buffer, 0, lengthBytes, 0, 4);

            int expectedMessageLength = BitConverter.ToInt32(lengthBytes);

            if (expectedMessageLength > this.buffer.Length)
            {
                throw new OutOfMemoryException("buffer to small");
            }

            if (expectedMessageLength > this.bytesReceived + receivedBytes)
            {
                //message hasn't completely arrived yet
                this.bytesReceived += receivedBytes;
                this.stream.BeginRead(this.buffer, this.bytesReceived, this.buffer.Length - this.bytesReceived, new AsyncCallback(OnRead), null);

            }
            else
            {
                //message completely arrived
                if (expectedMessageLength != this.bytesReceived + receivedBytes)
                {
                    Console.WriteLine("something has gone completely wrong");
                    Console.WriteLine($"expected: {expectedMessageLength} bytesReceive: {bytesReceived} receivedBytes: {receivedBytes}");
                }
                else if (buffer[4] == 0x02)
                {
                    Console.WriteLine($"received raw data {BitConverter.ToString(buffer.Skip(5).ToArray(), 16)}");
                }
                else if (buffer[4] == 0x01)
                {
                    byte[] packet = new byte[expectedMessageLength];
                    Console.WriteLine(Encoding.ASCII.GetString(buffer) + " " + expectedMessageLength);
                    Array.Copy(buffer, 5, packet, 0, expectedMessageLength - 5);
                    Console.WriteLine(Encoding.ASCII.GetString(packet));
                    HandleData(Encoding.ASCII.GetString(packet));
                }

            }
        }

        private void HandleData(string packet)
        {
            Console.WriteLine("Data " + packet);
            JsonConvert.DeserializeObject(packet);
        }
    }
}
