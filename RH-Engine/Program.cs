using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RH_Engine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            WriteTextMessage(client, "{\r\n\"id\" : \"session/list\"\r\n}");
            ReadPrefMessage(client.GetStream());
            Console.WriteLine("got response " + ReadTextMessage(client));
        }

        public static void WriteTextMessage(TcpClient client, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(GetPacketLength(msg.Length), 0, res, 0, 4);
            Array.Copy(msg, 0, res, 4, msg.Length);

            client.GetStream().Write(res);

            Console.WriteLine("sent message " + message);
        }

        public static string ReadTextMessage(TcpClient client)
        {
            var stream = new StreamReader(client.GetStream(), Encoding.ASCII);
            {
                Console.WriteLine("reading...");

                int length = stream.Read();
                Console.WriteLine(length);

                return "";
            }
        }

        private static byte[] GetPacketLength(int length)
        {
            Console.WriteLine("length got: " + length);
            byte[] packetLength = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                packetLength[i] = (byte)(length >> (8 * i));
            }

            Console.WriteLine("packet length: " + BitConverter.ToString(packetLength));
            return packetLength;
        }

        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            stream.Read(lengthBytes, 0, 4);

            int length = BitConverter.ToInt32(lengthBytes);

            byte[] buffer = new byte[length];
            int totalRead = 0;

            //read bytes until stream indicates there are no more

            int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
            totalRead += read;
            Console.WriteLine("ReadMessage: " + read);
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 4, length - 4));

            return Encoding.ASCII.GetString(buffer, 0, totalRead);
        }
    }
}