using System;
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

            CreateConnection(client.GetStream());

        }

        public static void WriteTextMessage(NetworkStream stream, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(BitConverter.GetBytes(msg.Length), 0, res, 0, 4);
            Array.Copy(msg, 0, res, 4, msg.Length);

            stream.Write(res);

            Console.WriteLine("sent message " + message);
        }

        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            stream.Read(lengthBytes, 0, 4);

            int length = BitConverter.ToInt32(lengthBytes);

            byte[] buffer = new byte[length];

            int read = stream.Read(buffer, 0, buffer.Length);


            return Encoding.UTF8.GetString(buffer);
        }

        private static void CreateConnection(NetworkStream stream)
        {
            //WriteTextMessage(stream, "{\r\n\"id\" : \"session/list\"\r\n}");
            //string msg = ReadPrefMessage(stream);
            //Console.WriteLine(msg);
            //string id = JSONParser.GetSessionID(msg, PCs);

            //Console.WriteLine(id);
            WriteTextMessage(stream, "{\r\n\"id\" : \"session/list\"\r\n}");
            string result = ReadPrefMessage(stream);
            Console.WriteLine(result);
            JSONParser.Parse(result);
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