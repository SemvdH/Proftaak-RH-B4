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

            
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            WriteTextMessage(client, "{\"id\" : \"session/list\"}");
            ReadPrefMessage(client.GetStream());
            Console.WriteLine("got response " + ReadTextMessage(client));
        }



        public static void WriteTextMessage(TcpClient client, string message)
        {

            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(GetPacketLength(msg.Length), 0,res, 0, 4);

            Array.Copy(msg, 0, res, 4, msg.Length);
            var stream = new StreamWriter(client.GetStream(), Encoding.Unicode);
            {
                stream.Write(res);
                stream.Flush();
            }

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
            byte[] packetLength = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                packetLength[i] = (byte)(length >> (8 * i));
            }

            return packetLength;
        }
        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            stream.Read(lengthBytes, 0, 4);

            int length = (int)(lengthBytes[0] | (lengthBytes[1] << 8) | (lengthBytes[1] << 16) | (lengthBytes[1] << 24));

            byte[] buffer = new byte[length];
            int totalRead = 0;

            //read bytes until stream indicates there are no more
            do
            {
                int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
                Console.WriteLine("ReadMessage: " + read);
            } while (stream.DataAvailable);

            return Encoding.ASCII.GetString(buffer, 0, totalRead);

        }

    }
}
