using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Util;

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
        public string username = null;
        private DateTime sessionStart;
        private string fileName;

        public Client(Communication communication, TcpClient tcpClient)
        {
            this.sessionStart = DateTime.Now;
            this.communication = communication;
            this.tcpClient = tcpClient;
            this.stream = this.tcpClient.GetStream();
            this.fileName = Directory.GetCurrentDirectory() + "/userInfo.dat";
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void OnRead(IAsyncResult ar)
        {
            if (ar == null || (!ar.IsCompleted) || (!this.stream.CanRead) || !this.tcpClient.Client.Connected)
                return;

            int receivedBytes = this.stream.EndRead(ar);

            if (totalBufferReceived + receivedBytes > 1024)
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

            if (ar == null || (!ar.IsCompleted) || (!this.stream.CanRead) || !this.tcpClient.Client.Connected)
                return;
            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);

        }

        private void OnWrite(IAsyncResult ar)
        {
            this.stream.EndWrite(ar);
        }


        /// <summary>
        /// handles all incoming data from the client
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
                        if (handleLogin(payloadbytes))
                            communication.NewLogin(this);
                        break;
                    case DataParser.LOGIN_DOCTOR:
                        if (communication.Doctor != null)
                            return;

                        if (handleLogin(payloadbytes))
                        {
                            communication.Doctor = this;
                        }
                        break;
                    case DataParser.START_SESSION:
                        this.communication.StartSessionUser(DataParser.getUsernameFromJson(payloadbytes));
                        break;
                    case DataParser.STOP_SESSION:
                        this.communication.StopSessionUser(DataParser.getUsernameFromJson(payloadbytes));
                        break;
                    case DataParser.SET_RESISTANCE:
                        //bool worked = DataParser.getResistanceFromResponseJson(payloadbytes);
                        communication.SendMessageToClient(DataParser.getUsernameFromJson(payloadbytes), message);
                        //set resistance on doctor GUI

                        break;
                    case DataParser.DISCONNECT:
                        communication.LogOff(this);
                        break;
                    case DataParser.MESSAGE:
                        communication.SendMessageToClient(DataParser.getUsernameFromJson(payloadbytes), message);
                        break;
                    default:
                        Console.WriteLine($"Received json with identifier {identifier}:\n{Encoding.ASCII.GetString(payloadbytes)}");
                        break;
                }
                saveData?.WriteDataJSON(Encoding.ASCII.GetString(payloadbytes));

                Array.Copy(message, 5, payloadbytes, 0, message.Length - 5);
                dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(payloadbytes));

            }
            else if (DataParser.isRawDataBikeServer(message))
            {
                saveData?.WriteDataRAWBike(payloadbytes);

            }
            else if (DataParser.isRawDataBPMServer(message))
            {
                saveData?.WriteDataRAWBPM(payloadbytes);
            }
               
        }

        private bool handleLogin(byte[] payloadbytes)
        {
            string username;
            string password;
            bool worked = DataParser.GetUsernamePassword(payloadbytes, out username, out password);
            if (worked)
            {
                if (verifyLogin(username, password))
                {
                    Console.WriteLine("Log in");
                    this.username = username;
                    sendMessage(DataParser.getLoginResponse("OK"));
                    return true;
                }
                else
                {
                    sendMessage(DataParser.getLoginResponse("wrong username or password"));
                }
            }
            else
            {
                sendMessage(DataParser.getLoginResponse("invalid json"));
            }
            return false;
        }

        public void sendMessage(byte[] message)
        {
            stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        private bool verifyLogin(string username, string password)
        {
            Console.WriteLine($"Got username {username} and password {password}");


            if (!File.Exists(fileName))
            {
                File.Create(fileName);
                Console.WriteLine("file doesnt exist");
                newUsers(username, password);
                Console.WriteLine("true");
                return true;
            }
            else
            {
                Console.WriteLine("file exists, located at " + Path.GetFullPath(fileName));
                string[] usernamesPasswords = File.ReadAllLines(fileName);
                foreach (string s in usernamesPasswords)
                {
                    string[] combo = s.Split(" ");
                    if (combo[0] == username)
                    {
                        Console.WriteLine("username found in file");
                        return combo[1] == password;
                    }

                }
                Console.WriteLine("username not found in file");
                newUsers(username, password);
                return true;


            }


        }

        private void newUsers(string username, string password)
        {

            Console.WriteLine("creating new entry in file");
            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(username + " " + password);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public void StartSession()
        {
            this.saveData = new SaveData(Directory.GetCurrentDirectory() + "/" + this.username + "/" + sessionStart.ToString("yyyy-MM-dd HH-mm-ss"));
        }

        public void StopSession()
        {
            this.saveData = null;
        }
    }
}
