using System;
using System.Collections.Generic;
using System.Text;
using RH_Engine;
using System.Net.Sockets;

namespace Client
{
    public delegate void HandleSerial(string message);

    public sealed class EngineConnect
    {
        private static EngineConnect instance = null;
        private static readonly object padlock = new object();


        private static PC[] PCs = {
            //new PC("DESKTOP-M2CIH87", "Fabian"),
            //new PC("T470S", "Shinichi"),
            //new PC("DESKTOP-DHS478C", "semme"),
            new PC("HP-ZBOOK-SEM", "Sem"),
            //new PC("DESKTOP-TV73FKO", "Wouter"),
            new PC("DESKTOP-SINMKT1", "Ralf van Aert"),
            //new PC("NA", "Bart")
        };

        private static ServerResponseReader serverResponseReader;
        private static string sessionId = string.Empty;
        private static string tunnelId = string.Empty;
        private static string routeId = string.Empty;
        private static string panelId = string.Empty;
        private static string bikeId = string.Empty;

        private static Dictionary<string, HandleSerial> serialResponses = new Dictionary<string, HandleSerial>();


        EngineConnect()
        {

        }

        public static EngineConnect INSTANCE
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new EngineConnect();
                    }
                }return instance;
            }
        }

        public static void Connect()
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            CreateConnection(client.GetStream());
        }

        /// <summary>
        /// connects to the server and creates the tunnel
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        private static void CreateConnection(NetworkStream stream)
        {
            initReader(stream);

            WriteTextMessage(stream, "{\r\n\"id\" : \"session/list\",\r\n\"serial\" : \"list\"\r\n}");

            // wait until we have got a sessionId
            while (sessionId == string.Empty) { }

            string tunnelCreate = "{\"id\" : \"tunnel/create\",	\"data\" :	{\"session\" : \"" + sessionId + "\"}}";

            WriteTextMessage(stream, tunnelCreate);

            // wait until we have a tunnel id
            while (tunnelId == string.Empty) { }
            Console.WriteLine("got tunnel id! sending commands...");
            sendCommands(stream, tunnelId);
        }
    }

    /// <summary>
    /// initializes and starts the reading of the responses from the vr server
    /// </summary>
    /// <param name="stream">the networkstream</param>
    private static void initReader(NetworkStream stream)
    {
        serverResponseReader = new ServerResponseReader(stream);
        serverResponseReader.callback = HandleResponse;
        serverResponseReader.StartRead();
    }
}
