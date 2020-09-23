using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private int bytesReceived;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //connect fiets?


            Client client = new Client();

        }

        public Client() : this("localhost", 5555)
        {

        }

        public Client(string adress, int port)
        {
            this.client = new TcpClient();
            this.bytesReceived = 0;
            client.BeginConnect(adress, port, new AsyncCallback(OnConnect), null);
        }

        private void OnConnect(IAsyncResult ar)
        {
            this.client.EndConnect(ar);
            Console.WriteLine("Verbonden!");
            this.stream = this.client.GetStream();

            //TODO File in lezen
            Console.WriteLine("enter username");
            string username = Console.ReadLine();
            Console.WriteLine("enter password");
            string password = Console.ReadLine();

            byte[] payload = DataParser.GetLoginJson(username, password);

            this.stream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(onWrite), null);

            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(onRead), null);
        }

        private void onRead(IAsyncResult ar)
        {
            int receivedBytes = this.stream.EndRead(ar);
            byte[] lengthBytes = new byte[4];

            Array.Copy(this.buffer, 0, lengthBytes, 0, 4);

            int expectedMessageLength = BitConverter.ToInt32(lengthBytes);

            if (expectedMessageLength > this.buffer.Length)
            {
                throw new OutOfMemoryException("buffer to small");
            }

            if (expectedMessageLength > this.bytesReceived + receivedBytes)
            {
                //message hasn't completely arrived yet
                this.bytesReceived += receivedBytes;
                this.stream.BeginRead(this.buffer, this.bytesReceived, this.buffer.Length - this.bytesReceived, new AsyncCallback(onRead), null);

            }
            else
            {
                //message completely arrived
                if (expectedMessageLength != this.bytesReceived + receivedBytes)
                {
                    Console.WriteLine("something has gone completely wrong");
                }

                string identifier;
                bool isJson = DataParser.getJsonIdentifier(this.buffer, out identifier);
                if (isJson)
                {
                    throw new NotImplementedException();
                }
                else if (DataParser.isRawData(this.buffer))
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void onWrite(IAsyncResult ar)
        {
            this.stream.EndWrite(ar);
            //stuff idk
        }
    }
}
