using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ClientApp.ViewModels;
using ProftaakRH;
using Util;

namespace ClientApp.Utils
{
    public delegate void EngineCallback();
    public class Client : IDataReceiver, IDisposable
    {
        public EngineCallback engineConnectFailed;
        public EngineCallback engineConnectSuccess;
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer = new byte[1024];
        private bool connected;
        private byte[] totalBuffer = new byte[1024];
        private int totalBufferReceived = 0;
        public EngineConnection engineConnection;
        private bool sessionRunning = false;
        private IHandler handler = null;
        private LoginViewModel LoginViewModel;


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
        /// initializes the VR engine and sets the callbacks
        /// </summary>
        private void initEngine()
        {
            Debug.WriteLine("init engine");
            engineConnection = EngineConnection.INSTANCE;
            engineConnection.OnNoTunnelId = RetryEngineConnection;
            engineConnection.OnSuccessFullConnection = engineConnected;
            engineConnection.OnEngineDisconnect = RetryEngineConnection;
            if (!engineConnection.Connected) engineConnection.Connect();
        }

        /// <summary>
        /// retries to connect to the VR engine if no tunnel id was found
        /// </summary>
        public void RetryEngineConnection()
        {
            engineConnectFailed?.Invoke();

        }

        private void engineConnected()
        {
            Console.WriteLine("successfully connected to VR engine");
            engineConnectSuccess?.Invoke();
            engineConnection.initScene();
            if (engineConnection.Connected && sessionRunning && !engineConnection.FollowingRoute) engineConnection.StartRouteFollow();
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
            Debug.WriteLine("got message in client app");
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
                                initEngine();

                            }
                            else
                            {
                                this.LoginViewModel.setLoginStatus(false);
                                Debug.WriteLine($"login failed \"{responseStatus}\"");
                            }
                            break;
                        case DataParser.START_SESSION:
                            Debug.WriteLine("Session started!");
                            this.sessionRunning = true;
                            if (engineConnection.Connected && !engineConnection.FollowingRoute) engineConnection.StartRouteFollow();
                            Debug.WriteLine("start");
                            break;
                        case DataParser.STOP_SESSION:
                            Console.WriteLine("Stop session identifier");
                            this.sessionRunning = false;
                            Debug.WriteLine("stop");
                            break;
                        case DataParser.SET_RESISTANCE:
                            Debug.WriteLine("Set resistance identifier");
                            if (this.handler == null)
                            {
                                // send that the operation was not successful if the handler is null
                                Debug.WriteLine("handler is null");
                                sendMessage(DataParser.getSetResistanceResponseJson(false));
                            }
                            else
                            {
                                // set the resistance in the vr scene and send that it was successful
                                float resistance = DataParser.getResistanceFromJson(payloadbytes);
                                Debug.WriteLine("resistance set was " + resistance);
                                this.handler.setResistance(resistance);
                                engineConnection.BikeResistance = resistance;
                                sendMessage(DataParser.getSetResistanceResponseJson(true));
                            }
                            break;
                        case DataParser.MESSAGE:
                            Debug.WriteLine("client has received message from doctor");
                            engineConnection.DoctorMessage = DataParser.getChatMessageFromJson(payloadbytes);
                            break;
                        case DataParser.NEW_CONNECTION:
                            this.LoginViewModel.DoctorConnected(DataParser.getUsernameFromJson(payloadbytes));
                            break;
                        case DataParser.DISCONNECT:
                            this.LoginViewModel.DoctorDisconnected(DataParser.getUsernameFromJson(payloadbytes));
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

        #region interface
        //maybe move this to other place
        /// <summary>
        /// bpm method for receiving the BPM value from the bluetooth bike or the simulation
        /// </summary>
        /// <param name="bytes">the message</param>
        public void BPM(byte[] bytes)
        {
            if (!sessionRunning)
            {
                return;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("no bytes");
            }
            byte[] message = DataParser.GetRawDataMessage(bytes);

            if (engineConnection.Connected && engineConnection.FollowingRoute)
            {
                engineConnection.BikeBPM = bytes[1];

            }


            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        /// <summary>
        /// method for receiving the bike message from the bluetooth bike or the simulation
        /// </summary>
        /// <param name="bytes">the message</param>
        public void Bike(byte[] bytes)
        {

            if (!sessionRunning)
            {
                return;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("no bytes");
            }
            byte[] message = DataParser.GetRawDataMessage(bytes);
            bool canSendToEngine = engineConnection.Connected && engineConnection.FollowingRoute;
            switch (bytes[0])
            {

                case 0x10:

                    if (canSendToEngine) engineConnection.BikeSpeed = (bytes[4] | (bytes[5] << 8)) * 0.01f;
                    break;
                case 0x19:
                    if (canSendToEngine) engineConnection.BikePower = (bytes[5]) | (bytes[6] & 0b00001111) << 8;
                    break;
            }


            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        #endregion

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

            byte[] message = DataParser.getJsonMessage(DataParser.GetLoginJson(username, hashPassword));


            this.stream.BeginWrite(message, 0, message.Length, new AsyncCallback(OnWrite), null);
        }

        /// <summary>
        /// sets the handler for the client, so either the bike simulator or the bluetooth bike handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetHandler(IHandler handler)
        {
            this.handler = handler;
        }

        internal void SetLoginViewModel(LoginViewModel loginViewModel)
        {
            this.LoginViewModel = loginViewModel;
        }

        public void Dispose()
        {
            Debug.WriteLine("client dispose called");
            sendMessage(DataParser.getDisconnectJson(LoginViewModel.Username));
            this.stream.Dispose();
            this.client.Dispose();
            this.handler.stop();
            this.engineConnection.Stop();
        }
    }
}
