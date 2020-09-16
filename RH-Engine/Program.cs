using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RH_Engine
{

    internal class Program
    {
        private static PC[] PCs = {
            new PC("DESKTOP-M2CIH87", "Fabian"),
            new PC("T470S", "Shinichi"),
            new PC("DESKTOP-DHS478C", "Sem"),
            new PC("DESKTOP-TV73FKO", "Wouter"),
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
            Console.WriteLine("read message..");

            int length = BitConverter.ToInt32(lengthBytes);

            //Console.WriteLine("length is: " + length);

            byte[] buffer = new byte[length];
            int totalRead = 0;

            //read bytes until stream indicates there are no more
            do
            {
                int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
                //Console.WriteLine("ReadMessage: " + read);
            } while (totalRead < length);

            return Encoding.UTF8.GetString(buffer, 0, totalRead);
        }

        private static void CreateConnection(NetworkStream stream)
        {
            WriteTextMessage(stream, "{\r\n\"id\" : \"session/list\"\r\n}");
            string id = JSONParser.GetSessionID(ReadPrefMessage(stream), PCs);

            string tunnelCreate = "{\"id\" : \"tunnel/create\",	\"data\" :	{\"session\" : \"" + id + "\"}}";

            WriteTextMessage(stream, tunnelCreate);

            string tunnelResponse = ReadPrefMessage(stream);

            Console.WriteLine(tunnelResponse);
            
            string tunnelID = JSONParser.GetTunnelID(tunnelResponse);
            if (tunnelID == null)
            {
                Console.WriteLine("could not find a valid tunnel id!");
                return;
            }

            CreateGraphics createGraphics = new CreateGraphics(tunnelID);
            string command = createGraphics.SkyboxCommand(2);

            
            Console.WriteLine("tunnelID is: " + tunnelID);

            WriteTextMessage(stream, command);

            Console.WriteLine(ReadPrefMessage(stream));
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

        public override string ToString()
        {
            return "PC - host:" + host + " - user:" + user;
        }
    }
}