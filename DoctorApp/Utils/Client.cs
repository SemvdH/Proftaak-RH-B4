using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using DoctorApp.ViewModels;
using ProftaakRH;
using Util;

namespace DoctorApp.Utils
{
    public delegate void EngineCallback();
    public class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private bool connected;
        private byte[] totalBuffer = new byte[1024];
        private int totalBufferReceived = 0;
        private LoginViewModel LoginViewModel;
        private MainViewModel MainViewModel;
        private ClientInfoViewModel ClientInfoViewModel;


        public Client() : this("localhost", 5555)
        {

        }

        public Client(string adress, int port)
        {
            this.client = new TcpClient();
            this.connected = false;
            client.BeginConnect(adress, port, new AsyncCallback(OnConnect), null);
        }

        /// <summary>
        /// callback method for when the TCP client is connected
        /// </summary>
        /// <param name="ar">the result of the async read</param>
        private void OnConnect(IAsyncResult ar)
        {
            this.client.EndConnect(ar);
            Console.WriteLine("TCP client Verbonden!");

            this.stream = this.client.GetStream();

            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnRead), null);
        }

        /// <summary>
        /// callback method for when there is a message read
        /// </summary>
        /// <param name="ar">the result of the async read</param>
        private void OnRead(IAsyncResult ar)
        {
            if (ar == null || (!ar.IsCompleted) || (!this.stream.CanRead))
                return;

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
                                Debug.WriteLine("Username and password correct!");
                                this.LoginViewModel.setLoginStatus(true);
                                this.connected = true;

                            }
                            else
                            {
                                this.LoginViewModel.setLoginStatus(false);
                                Debug.WriteLine($"login failed \"{responseStatus}\"");
                            }
                            break;
                        case DataParser.START_SESSION:
                            Console.WriteLine("Session started!");
                            break;
                        case DataParser.STOP_SESSION:
                            Console.WriteLine("Stop session identifier");
                            break;
                        case DataParser.SET_RESISTANCE:
                            Console.WriteLine("Set resistance identifier");
                            break;
                        case DataParser.NEW_CONNECTION:
                            this.MainViewModel.NewConnectedUser(DataParser.getUsernameFromResponseJson(payloadbytes));
                            break;
                        case DataParser.DISCONNECT:
                            this.MainViewModel.DisconnectedUser(DataParser.getUsernameFromResponseJson(payloadbytes));
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

        /// <summary>
        /// starts sending a message to the server
        /// </summary>
        /// <param name="message">the message to send</param>
        public void sendMessage(byte[] message)
        {
            stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        /// <summary>
        /// callback method for when a message is fully written to the server
        /// </summary>
        /// <param name="ar">the async result representing the asynchronous call</param>
        private void OnWrite(IAsyncResult ar)
        {
            this.stream.EndWrite(ar);
        }

        /// <summary>
        /// wether or not the client stream is connected
        /// </summary>
        /// <returns>true if it's connected, false if not</returns>
        public bool IsConnected()
        {
            return this.connected;
        }
        /// <summary>
        /// tries to log in to the server by asking for a username and password
        /// </summary>
        public void tryLogin(string username, string password)
        {

            string hashPassword = Util.Hasher.HashString(password);

            byte[] message = DataParser.getJsonMessage(DataParser.LoginAsDoctor(username, hashPassword));


            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        internal void SetLoginViewModel(LoginViewModel loginViewModel)
        {
            this.LoginViewModel = loginViewModel;
        }

        internal void SetMainViewModel(MainViewModel mainViewModel)
        {
            this.MainViewModel = mainViewModel;
        }

        internal void SetClientInfoViewModel(ClientInfoViewModel clientInfoViewModel)
        {
            this.ClientInfoViewModel = clientInfoViewModel;
        }

        public void Dispose()
        {
            Debug.WriteLine("client dispose called");
            this.stream.Dispose();
            this.client.Dispose();
        }
    }
}
