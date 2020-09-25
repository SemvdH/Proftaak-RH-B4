using LibNoise.Primitive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace RH_Engine
{

    public delegate void HandleSerial(string uuid);
    internal class Program
    {
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

        private static Dictionary<string, HandleSerial> serialResponses = new Dictionary<string, HandleSerial>();

        
        private static void Main(string[] args)
        {

            serialResponses.Add("route",handleRouteSerial);

            TcpClient client = new TcpClient("145.48.6.10", 6666);

            CreateConnection(client.GetStream());
        }

        private static void handleRouteSerial(string uuid)
        {

        }

        private static void initReader(NetworkStream stream)
        {
            serverResponseReader = new ServerResponseReader(stream);
            serverResponseReader.callback = HandleResponse;
            serverResponseReader.StartRead();
        }

        public static void HandleResponse(string message)
        {

            string id = JSONParser.GetID(message);

            // because the first messages doesn't have a serial, we need to check on the id

            if (id == "session/list")
            {
                sessionId = JSONParser.GetSessionID(message,PCs);
            } else if (id == "tunnel/create")
            {
                tunnelId = JSONParser.GetTunnelID(message);
                if (tunnelId == null)
                {
                    Console.WriteLine("could not find a valid tunnel id!");
                    return;
                }
            }

            if (message.Contains("serial"))
            {
                string serial = JSONParser.GetSerial(message);
                Console.WriteLine("Got serial " + serial);
                serialResponses[serial].Invoke();
            }


        }

        /// <summary>
        /// writes a message to the server
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        /// <param name="message">the message to send</param>
        public static void WriteTextMessage(NetworkStream stream, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            byte[] res = new byte[msg.Length + 4];

            Array.Copy(BitConverter.GetBytes(msg.Length), 0, res, 0, 4);
            Array.Copy(msg, 0, res, 4, msg.Length);

            stream.Write(res);

            Console.WriteLine("sent message " + message);
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

        /// <summary>
        /// sends all the commands to the server
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        /// <param name="tunnelID">the tunnel id to use</param>
        private static void sendCommands(NetworkStream stream, string tunnelID)
        {
            Command mainCommand = new Command(tunnelID);


            WriteTextMessage(stream, mainCommand.ResetScene());
            string routeid = CreateRoute(stream, mainCommand);

            //WriteTextMessage(stream, mainCommand.TerrainCommand(new int[] { 256, 256 }, null));
            //string command;



            //command = mainCommand.AddBikeModel();

            //WriteTextMessage(stream, command);

            //Console.WriteLine(ReadPrefMessage(stream));

            //command = mainCommand.AddModel("car", "data\\customModels\\TeslaRoadster.fbx");

            //WriteTextMessage(stream, command);

            //Console.WriteLine(ReadPrefMessage(stream));


            //command = mainCommand.addPanel();
            //WriteTextMessage(stream, command);
            //string response = ReadPrefMessage(stream);
            //Console.WriteLine("add Panel response: \n\r" + response);
            //string uuidPanel = JSONParser.getPanelID(response);
            //WriteTextMessage(stream, mainCommand.ClearPanel(uuidPanel));
            //Console.WriteLine(ReadPrefMessage(stream));
            //WriteTextMessage(stream, mainCommand.bikeSpeed(uuidPanel));
            //Console.WriteLine(ReadPrefMessage(stream));

            //WriteTextMessage(stream, mainCommand.SwapPanelCommand(uuidPanel));
        }

        /// <summary>
        /// gets the id of the object with the given name
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns> the uuid of the object with the given name, <c>null</c> otherwise.</returns>
        public static string GetId(string name, NetworkStream stream, Command createGraphics)
        {
            JArray children = GetChildren(stream, createGraphics);

            foreach (dynamic child in children)
            {
                if (child.name == name)
                {
                    return child.uuid;
                }
            }
            Console.WriteLine("Could not find id of " + name);
            return null;

        }

        public static string CreateRoute(NetworkStream stream, Command createGraphics)
        {
            //=============================================================================================================TODO change  
            WriteTextMessage(stream, createGraphics.RouteCommand());
            //dynamic response = JsonConvert.DeserializeObject(ReadPrefMessage(stream));
            //dynamic response = null;
            //if (response.data.data.id == "route/add")
            //{
            //    return response.data.data.data.uuid;
            //}
            return null;

        }

        public static void CreateTerrain(NetworkStream stream, Command createGraphics)
        {
            float x = 0f;
            float[] height = new float[256 * 256];
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0, LibNoise.NoiseQuality.Best);
            for (int i = 0; i < 256 * 256; i++)
            {
                height[i] = improvedPerlin.GetValue(x / 10, x / 10, x * 100) + 1;
                x += 0.001f;
            }

            //=============================================================================================================TODO change  
            WriteTextMessage(stream, createGraphics.TerrainCommand(new int[] { 256, 256 }, height));
            //Console.WriteLine(ReadPrefMessage(stream));

            WriteTextMessage(stream, createGraphics.AddNodeCommand());
            //Console.WriteLine(ReadPrefMessage(stream));
        }

        /// <summary>
        /// gets all the children in the current scene
        /// </summary>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns>all the children objects in the current scene</returns>
        public static JArray GetChildren(NetworkStream stream, Command createGraphics)
        {
            WriteTextMessage(stream, createGraphics.GetSceneInfoCommand());
            //dynamic response = JsonConvert.DeserializeObject(ReadPrefMessage(stream));
            //return response.data.data.data.children;
            //=============================================================================================================TODO change            
            return null;
        }

        /// <summary>
        /// returns all objects in the current scene, as name-uuid tuples.
        /// </summary>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns>an array of name-uuid tuples for each object</returns>
        public static (string, string)[] GetObjectsInScene(NetworkStream stream, Command createGraphics)
        {
            JArray children = GetChildren(stream, createGraphics);
            (string, string)[] res = new (string, string)[children.Count];

            int i = 0;
            foreach (dynamic child in children)
            {
                res[i] = (child.name, child.uuid);
                i++;
            }

            return res;

        }

        public static string getUUIDFromResponse(string response)
        {
            dynamic JSON = JsonConvert.DeserializeObject(response);
            return JSON.data.data.data.uuid;
        }

    }

    /// <summary>
    /// struct used to store the host pc name and user
    /// </summary>
    public readonly struct PC
    {
        public PC(string host, string user)
        {
            this.host = host;
            this.user = user;
        }
        public string host { get; }
        public string user { get; }

        public override string ToString()
        {
            return "PC - host:" + host + " - user:" + user;
        }
    }
}