using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Client;
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
        private int totalBufferReceived = 0;
        private SaveData saveData;
        private string username = null;
        private DateTime sessionStart;



        public string Username { get; set; }

        public Client(Communication communication, TcpClient tcpClient)
        {
            this.sessionStart = DateTime.Now;
            this.communication = communication;
            this.tcpClient = tcpClient;
            this.stream = this.tcpClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void OnRead(IAsyncResult ar)
        {
            int receivedBytes = this.stream.EndRead(ar);

            if (totalBufferReceived + receivedBytes > 1024)
            {
                throw new OutOfMemoryException("buffer too small");
            }
            Array.Copy(buffer, 0, totalBuffer, totalBufferReceived, receivedBytes);
            totalBufferReceived += receivedBytes;

            int expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
            while (totalBufferReceived >= expectedMessageLength)
            {
                //volledig packet binnen
                byte[] messageBytes = new byte[expectedMessageLength];
                Array.Copy(totalBuffer, 0, messageBytes, 0, expectedMessageLength);
                HandleData(messageBytes);

                Array.Copy(totalBuffer, expectedMessageLength, totalBuffer, 0, (totalBufferReceived - expectedMessageLength)); //maybe unsafe idk 

                totalBufferReceived -= expectedMessageLength;
                expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
                if (expectedMessageLength <= 5)
                {
                    break;
                }
            }

            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);

        }

        private void OnWrite(IAsyncResult ar)
        {
            this.stream.EndWrite(ar);
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message">including message length and messageId (can be changed)</param>
        private void HandleData(byte[] message)
        {
            //Console.WriteLine("Data " + packet);
            //JsonConvert.DeserializeObject(packet);
            //0x01 Json
            //0x01 Raw data

            byte[] payloadbytes = new byte[BitConverter.ToInt32(message, 0) - 5];

            Array.Copy(message, 5, payloadbytes, 0, payloadbytes.Length);

            string identifier;
            bool isJson = DataParser.getJsonIdentifier(message, out identifier);
            if (isJson)
            {
                switch (identifier)
                {
                    case DataParser.LOGIN:
                        string username;
                        string password;
                        bool worked = DataParser.GetUsernamePassword(payloadbytes, out username, out password);
                        if (worked)
                        {
                            if (verifyLogin(username, password))
                            {
                                Console.WriteLine("Log in");
                                this.username = username;
                                byte[] response = DataParser.getLoginResponse("OK");
                                stream.BeginWrite(response, 0, response.Length, new AsyncCallback(OnWrite), null);
                                this.saveData = new SaveData(Directory.GetCurrentDirectory() + "/" + username, sessionStart.ToString("yyyy-MM-dd HH-mm-ss"));
                            }
                            else
                            {
                                byte[] response = DataParser.getLoginResponse("wrong username or password");
                                stream.BeginWrite(response, 0, response.Length, new AsyncCallback(OnWrite), null);
                            }
                        }
                        else
                        {
                            byte[] response = DataParser.getLoginResponse("invalid json");
                            stream.BeginWrite(response, 0, response.Length, new AsyncCallback(OnWrite), null);
                        }
                        break;
                    default:
                        Console.WriteLine($"Received json with identifier {identifier}:\n{Encoding.ASCII.GetString(payloadbytes)}");
                        break;
                }
                dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(payloadbytes));
                saveData.WriteDataJSON(Encoding.ASCII.GetString(payloadbytes));

            }
            else if (DataParser.isRawData(message))
            {
                Console.WriteLine(BitConverter.ToString(message));
                saveData.WriteDataRAW(message);
            }


        }

        private bool verifyLogin(string username, string password)
        {
            return username == password;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
