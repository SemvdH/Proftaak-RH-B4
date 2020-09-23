using System;
using System.Collections.Generic;
using System.IO;
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
            try
            {
                int receivedBytes = stream.EndRead(ar);
            }
            catch (IOException)
            {
                communication.Disconnect(this);
                return;
            }

            int counter = 0;

            while (buffer.Length > counter)
            {
                //Console.WriteLine(buffer.Length);
                byte[] lenghtBytes = new byte[4];
                Array.Copy(buffer, counter, lenghtBytes, 0, 4);
                int length = BitConverter.ToInt32(lenghtBytes);
                Console.WriteLine(buffer[5]);
                if (length == 0)
                {
                    break;
                }
                else if(buffer[counter+4]==0x02)
                {

                }
                else if(buffer[counter+4]==0x01)
                {
                    byte[] packet = new byte[length];
                    Console.WriteLine(Encoding.ASCII.GetString(buffer)+" "+length);
                    Array.Copy(buffer, counter+5, packet, 0, length);
                    Console.WriteLine(Encoding.ASCII.GetString(packet));
                    HandleData(Encoding.ASCII.GetString(packet));
                }
                
                counter += length;
            }

            Console.WriteLine("Done");

            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void HandleData(string packet)
        {
            Console.WriteLine("Data "+packet);
            JsonConvert.DeserializeObject(packet);
        }
    }
}
