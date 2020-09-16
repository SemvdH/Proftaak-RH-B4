﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace RH_Engine
{

    internal class Program
    {
        private static PC[] PCs = { 
            new PC("DESKTOP-M2CIH87", "Fabian"),
            new PC("T470S", "Shinichi"),
            new PC("NA", "Sem"),
            new PC("NA", "Wouter"),
            new PC("NA", "Ralf"),
            new PC("NA", "Bart") };
        private static void Main(string[] args)
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            WriteTextMessage(client, "{\r\n\"id\" : \"session/list\"\r\n}");
            string result = ReadPrefMessage(client.GetStream());
            JSONParser.Parse(result);

        }

        public static void WriteTextMessage(TcpClient client, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(BitConverter.GetBytes(msg.Length), 0, res, 0, 4);
            Array.Copy(msg, 0, res, 4, msg.Length);

            client.GetStream().Write(res);

            Console.WriteLine("sent message " + message);
        }

        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            stream.Read(lengthBytes, 0, 4);

            int length = BitConverter.ToInt32(lengthBytes);

            byte[] buffer = new byte[length];
            int totalRead = 0;

            int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
            totalRead += read;
            //Console.WriteLine("ReadMessage: " + read);
            //Console.WriteLine(Encoding.UTF8.GetString(buffer));

            return Encoding.UTF8.GetString(buffer);
        }

        private static void CreateTunnel()
        {

        }
    }


    public readonly struct PC
    {
        public PC(string host, string user)
        {
            this.host = host;
            this.user = user;
        }
        public string host { get; }
        public string user { get; }
    }
}