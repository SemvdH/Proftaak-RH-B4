using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RH_Engine
{
    class Program
    {
        static void Main(string[] args)
        {

            // 0x1B,0x00,0x00,0x00
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            WriteTextMessage(client, "{\"id\":\"session/list\"}");
            Console.WriteLine("got response " + ReadTextMessage(client));
        }



        public static void WriteTextMessage(TcpClient client, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(res, 0, GetPacketLength(msg.Length), 0, 4);

            Array.Copy(res, 4, msg, 0, msg.Length);
            var stream = new StreamWriter(client.GetStream(), Encoding.Default);
            stream.Write(res);
            stream.Flush();
        }

        public static string ReadTextMessage(TcpClient client)
        {
            var stream = new StreamReader(client.GetStream(), Encoding.ASCII);
            {
                return stream.ReadLine();
            }
        }

        private static byte[] GetPacketLength(int length)
        {
            byte[] packetLength = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                packetLength[i] = (byte)(length >> (8 * i));
            }

            return packetLength;
        }
    }
}
