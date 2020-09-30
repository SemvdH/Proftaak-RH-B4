using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProftaakRH;

namespace Client
{
    class Client : IDataReceiver
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private bool connected;
        private byte[] totalBuffer = new byte[1024];
        private int totalBufferReceived = 0;
        private EngineConnection engineConnection;


        public Client() : this("localhost", 5555)
        {

        }

        public Client(string adress, int port)
        {
            this.client = new TcpClient();
            this.connected = false;
            client.BeginConnect(adress, port, new AsyncCallback(OnConnect), null);
            initEngine();

        }

        private void initEngine()
        {
            engineConnection = EngineConnection.INSTANCE;
            engineConnection.Connect();

        }

        private void OnConnect(IAsyncResult ar)
        {
            this.client.EndConnect(ar);
            Console.WriteLine("Verbonden!");


            this.stream = this.client.GetStream();

            tryLogin();

            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);
        }

        private void OnRead(IAsyncResult ar)
        {
            int receivedBytes = this.stream.EndRead(ar);

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


                byte[] payloadbytes = new byte[BitConverter.ToInt32(messageBytes, 0) - 5];

                Array.Copy(messageBytes, 5, payloadbytes, 0, payloadbytes.Length);

                string identifier;
                bool isJson = DataParser.getJsonIdentifier(messageBytes, out identifier);
                if (isJson)
                {
                    switch (identifier)
                    {
                        case DataParser.LOGIN_RESPONSE:
                            string responseStatus = DataParser.getResponseStatus(payloadbytes);
                            if (responseStatus == "OK")
                            {
                                this.connected = true;
                            }
                            else
                            {
                                Console.WriteLine($"login failed \"{responseStatus}\"");
                                tryLogin();
                            }
                            break;
                        default:
                            Console.WriteLine($"Received json with identifier {identifier}:\n{Encoding.ASCII.GetString(payloadbytes)}");
                            break;
                    }
                }
                else if (DataParser.isRawData(messageBytes))
                {
                    Console.WriteLine($"Received data: {BitConverter.ToString(payloadbytes)}");
                }

                totalBufferReceived -= expectedMessageLength;
                expectedMessageLength = BitConverter.ToInt32(totalBuffer, 0);
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
        private void tryLogin()
        {
            //TODO File in lezen
            Console.WriteLine("enter username");
            string username = Console.ReadLine();
            Console.WriteLine("enter password");
            string password = Console.ReadLine();

            byte[] message = DataParser.getJsonMessage(DataParser.GetLoginJson(username, password));

            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }
    }
}
