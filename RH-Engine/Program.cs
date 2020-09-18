using LibNoise.Primitive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace RH_Engine
{

    internal class Program
    {
        private static PC[] PCs = {
            //new PC("DESKTOP-M2CIH87", "Fabian"),
            new PC("T470S", "Shinichi"),
            //new PC("DESKTOP-DHS478C", "semme")
            //new PC("DESKTOP-TV73FKO", "Wouter"),
            //new PC("DESKTOP-SINMKT1", "Ralf"),
            //new PC("NA", "Bart")
        };
        private static void Main(string[] args)
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            CreateConnection(client.GetStream());


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

            //Console.WriteLine("sent message " + message);
        }

        /// <summary>
        /// reads a response from the server
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        /// <returns>the returned message from the server</returns>
        public static string ReadPrefMessage(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];

            stream.Read(lengthBytes, 0, 4);
            Console.WriteLine("read message..");

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
        /// <summary>
        /// connects to the server and creates the tunnel
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        private static void CreateConnection(NetworkStream stream)
        {
            WriteTextMessage(stream, "{\r\n\"id\" : \"session/list\"\r\n}");
            string id = JSONParser.GetSessionID(ReadPrefMessage(stream), PCs);

            string tunnelCreate = "{\"id\" : \"tunnel/create\",	\"data\" :	{\"session\" : \"" + id + "\"}}";

            WriteTextMessage(stream, tunnelCreate);

            string tunnelResponse = ReadPrefMessage(stream);

            Console.WriteLine(tunnelResponse);

            string tunnelID = JSONParser.GetTunnelID(tunnelResponse);
            if (tunnelID == null)
            {
                Console.WriteLine("could not find a valid tunnel id!");
                return;
            }

            CreateGraphics createGraphics = new CreateGraphics(tunnelID);


            WriteTextMessage(stream, createGraphics.ResetScene());
            ReadPrefMessage(stream);
            string routeid = CreateRoute(stream, createGraphics);

            WriteTextMessage(stream, createGraphics.TerrainCommand(new int[] { 256, 256 }, null));
            Console.WriteLine(ReadPrefMessage(stream));
            string command;

            command = createGraphics.AddBikeModel();

            WriteTextMessage(stream, command);

            Console.WriteLine(ReadPrefMessage(stream));

            command = createGraphics.AddModel("car", "data\\customModels\\TeslaRoadster.fbx");

            WriteTextMessage(stream, command);

            Console.WriteLine(ReadPrefMessage(stream));



        }

        /// <summary>
        /// gets the id of the object with the given name
        /// </summary>
        /// <param name="name">the name of the object</param>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns> the uuid of the object with the given name, <c>null</c> otherwise.</returns>
        public static string GetId(string name, NetworkStream stream, CreateGraphics createGraphics)
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

        public static string CreateRoute(NetworkStream stream, CreateGraphics createGraphics)
        {
            WriteTextMessage(stream, createGraphics.RouteCommand());
            dynamic response = JsonConvert.DeserializeObject(ReadPrefMessage(stream));
            if (response.data.data.id == "route/add")
            {
                return response.data.data.data.uuid;
            }
            return null;

        }

        public static void CreateTerrain(NetworkStream stream, CreateGraphics createGraphics)
        {
            float x = 0f;
            float[] height = new float[256 * 256];
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0, LibNoise.NoiseQuality.Best);
            for (int i = 0; i < 256 * 256; i++)
            {
                height[i] = improvedPerlin.GetValue(x / 10, x / 10, x * 100) + 1;
                x += 0.001f;
            }

            WriteTextMessage(stream, createGraphics.TerrainCommand(new int[] { 256, 256 }, height));
            Console.WriteLine(ReadPrefMessage(stream));

            WriteTextMessage(stream, createGraphics.AddNodeCommand());
            Console.WriteLine(ReadPrefMessage(stream));
        }

        /// <summary>
        /// gets all the children in the current scene
        /// </summary>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns>all the children objects in the current scene</returns>
        public static JArray GetChildren(NetworkStream stream, CreateGraphics createGraphics)
        {
            WriteTextMessage(stream, createGraphics.GetSceneInfoCommand());
            dynamic response = JsonConvert.DeserializeObject(ReadPrefMessage(stream));
            return response.data.data.data.children;
        }

        /// <summary>
        /// returns all objects in the current scene, as name-uuid tuples.
        /// </summary>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns>an array of name-uuid tuples for each object</returns>
        public static (string, string)[] GetObjectsInScene(NetworkStream stream, CreateGraphics createGraphics)
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