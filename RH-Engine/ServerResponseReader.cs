using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RH_Engine
{
    public delegate void OnResponse(string response);

    public class ServerResponseReader
    {
        public OnResponse callback
        {
            get; set;
        }

        public NetworkStream Stream { get; }

        public ServerResponseReader(NetworkStream stream)
        {
            this.Stream = stream;
        }

        public void StartRead()
        {
            Thread t = new Thread(() =>
            {
                if (this.callback == null)
                {
                    throw new Exception("Callback not initialized!");
                }
                else
                {
                    Console.WriteLine("[SERVERRESPONSEREADER] Starting loop for reading");
                    while (true)
                    {
                        string res = ReadPrefMessage(Stream);
                        //Console.WriteLine("[SERVERRESPONSEREADER] got message from server: " + res);
                        this.callback(res);
                    }
                }
            });

            t.Start();
        }

        /// <summary>
        /// reads a response from the server
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        /// <returns>the returned message from the server</returns>
        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            int streamread = stream.Read(lengthBytes, 0, 4);
            //Console.WriteLine("read message.. " + streamread);

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
    }
}