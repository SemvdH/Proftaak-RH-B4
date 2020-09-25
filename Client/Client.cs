using System;
using System.Net.Sockets;
using ProftaakRH;

namespace Client
{
    class Client : IDataReceiver
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private int bytesReceived;
        private bool connected;


        public Client() : this("localhost", 5555)
        {

        }

        public Client(string adress, int port)
        {
            this.client = new TcpClient();
            this.bytesReceived = 0;
            this.connected = false;
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

            byte[] message = DataParser.getJsonMessage(DataParser.GetLoginJson(username, password));

            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);

            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);

            //TODO lees OK message
            //temp moet eigenlijk een ok bericht ontvangen
            this.connected = true;
        }

        private void OnRead(IAsyncResult ar)
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
                this.stream.BeginRead(this.buffer, this.bytesReceived, this.buffer.Length - this.bytesReceived, new AsyncCallback(OnRead), null);

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
            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);

        }

        private void OnWrite(IAsyncResult ar)
        {
            this.stream.EndWrite(ar);
        }

        #region interface
        //maybe move this to other place
        public void BPM(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("no bytes");
            }
            byte[] message = DataParser.GetRawDataMessage(bytes);
            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        public void Bike(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("no bytes");
            }
            byte[] message = DataParser.GetRawDataMessage(bytes);
            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        #endregion

        public bool IsConnected()
        {
            return this.connected;
        }
    }
}
