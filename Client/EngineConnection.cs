﻿using System;
using System.Collections.Generic;
using System.Text;
using RH_Engine;
using System.Net.Sockets;

namespace Client
{
    public delegate void HandleSerial(string message);
    public delegate void HandleNoTunnelId();

    public sealed class EngineConnection
    {
        private static EngineConnection instance = null;
        private static readonly object padlock = new object();
        public HandleNoTunnelId OnNoTunnelId;


        private static PC[] PCs = {
            //new PC("DESKTOP-M2CIH87", "Fabian"),
            //new PC("T470S", "Shinichi"),
            //new PC("DESKTOP-DHS478C", "semme"),
            new PC("HP-ZBOOK-SEM", "Sem")
            //new PC("DESKTOP-TV73FKO", "Wouter"),
            //new PC("DESKTOP-SINMKT1", "Ralf van Aert"),
            //new PC("NA", "Bart")
        };

        private static ServerResponseReader serverResponseReader;
        private static string sessionId = string.Empty;
        private static string tunnelId = string.Empty;
        private static string routeId = string.Empty;
        private static string panelId = string.Empty;
        private static string bikeId = string.Empty;

        private static NetworkStream stream;

        private static Dictionary<string, HandleSerial> serialResponses = new Dictionary<string, HandleSerial>();
        private Command mainCommand;

        public bool Connected = false;

        EngineConnection()
        {

        }

        public static EngineConnection INSTANCE
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new EngineConnection();
                    }
                }
                return instance;
            }
        }

        public void Connect()
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);
            stream = client.GetStream();
            initReader();
            CreateConnection();
        }

        /// <summary>
        /// connects to the server and creates the tunnel
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        public void CreateConnection()
        {

            WriteTextMessage( "{\r\n\"id\" : \"session/list\",\r\n\"serial\" : \"list\"\r\n}");

            // wait until we have got a sessionId
            while (sessionId == string.Empty) { }

            string tunnelCreate = "{\"id\" : \"tunnel/create\",	\"data\" :	{\"session\" : \"" + sessionId + "\"}}";

            WriteTextMessage(tunnelCreate);

            // wait until we have a tunnel id
            while (tunnelId == string.Empty) { }
            Write("got tunnel id! " + tunnelId);
            mainCommand = new Command(tunnelId);

        }
        /// <summary>
        /// initializes and starts the reading of the responses from the vr server
        /// </summary>
        /// <param name="stream">the networkstream</param>
        private void initReader()
        {
            serverResponseReader = new ServerResponseReader(stream);
            serverResponseReader.callback = HandleResponse;
            serverResponseReader.StartRead();
            Connected = true;
        }

        /// <summary>
        /// callback method that handles responses from the server
        /// </summary>
        /// <param name="message">the response message from the server</param>
        public void HandleResponse(string message)
        {
            string id = JSONParser.GetID(message);

            // because the first messages don't have a serial, we need to check on the id
            if (id == "session/list")
            {
                sessionId = JSONParser.GetSessionID(message, PCs);
            }
            else if (id == "tunnel/create")
            {
                tunnelId = JSONParser.GetTunnelID(message);
                if (tunnelId == null)
                {
                    Write("could not find a valid tunnel id!");
                    OnNoTunnelId?.Invoke();
                    Connected = false;
                    return;
                }
            }

            if (message.Contains("serial"))
            {
                //Console.WriteLine("GOT MESSAGE WITH SERIAL: " + message + "\n\n\n");
                string serial = JSONParser.GetSerial(message);
                //Console.WriteLine("Got serial " + serial);
                if (serialResponses.ContainsKey(serial)) serialResponses[serial].Invoke(message);
            }
        }

        /// <summary>
        /// method that sends the speciefied message with the specified serial, and executes the given action upon receivind a reply from the server with this serial.
        /// </summary>
        /// <param name="stream">the networkstream to use</param>
        /// <param name="message">the message to send</param>
        /// <param name="serial">the serial to check for</param>
        /// <param name="action">the code to be executed upon reveiving a reply from the server with the specified serial</param>
        public void SendMessageAndOnResponse(string message, string serial, HandleSerial action)
        {
            serialResponses.Add(serial, action);
            WriteTextMessage(message);
        }

        /// <summary>
        /// writes a message to the server
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        /// <param name="message">the message to send</param>
        public void WriteTextMessage(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(BitConverter.GetBytes(msg.Length), 0, res, 0, 4);
            Array.Copy(msg, 0, res, 4, msg.Length);

            stream.Write(res);

            Write("sent message " + message);
        }
        public void Write(string msg)
        {
            Console.WriteLine( "[ENGINECONNECT] " + msg);
        }

    }


}
