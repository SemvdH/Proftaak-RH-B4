using LibNoise.Primitive;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RH_Engine
{
    public delegate void HandleSerial(string message);

    class Program
    {
        private static PC[] PCs = {
            //new PC("DESKTOP-M2CIH87", "Fabian"),
            //new PC("T470S", "Shinichi"),
            //new PC("DESKTOP-DHS478C", "semme"),
            //new PC("HP-ZBOOK-SEM", "Sem"),
            //new PC("DESKTOP-TV73FKO", "Wouter"),
            new PC("DESKTOP-SINMKT1", "Ralf van Aert"),
            //new PC("NA", "Bart")
        };

        private static ServerResponseReader serverResponseReader;
        private static string sessionId = string.Empty;
        private static string tunnelId = string.Empty;
        private static string cameraId = string.Empty;
        private static string routeId = string.Empty;
        private static string panelId = string.Empty;
        private static string bikeId = string.Empty;
        private static string headId = string.Empty;

        private static Dictionary<string, HandleSerial> serialResponses = new Dictionary<string, HandleSerial>();

        private static void Main(string[] args)
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);

            CreateConnection(client.GetStream());
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

        /// <summary>
        /// callback method that handles responses from the server
        /// </summary>
        /// <param name="message">the response message from the server</param>
        public static void HandleResponse(string message)
        {
            //Console.WriteLine(message);
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
                    Console.WriteLine("could not find a valid tunnel id!");
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
        public static void SendMessageAndOnResponse(NetworkStream stream, string message, string serial, HandleSerial action)
        {
            serialResponses.Add(serial, action);
            WriteTextMessage(stream, message);
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

            // Reset scene
            WriteTextMessage(stream, mainCommand.ResetScene());
            //headId = GetId("Root", stream, mainCommand);
            //while (headId == string.Empty) { }

            //Get sceneinfo
            SendMessageAndOnResponse(stream, mainCommand.GetSceneInfoCommand("sceneinfo"), "sceneinfo",
                (message) =>
                {
                    //Console.WriteLine("\r\n\r\n\r\nscene info" + message);
                    cameraId = JSONParser.GetIdSceneInfoChild(message, "Camera");
                    string headId = JSONParser.GetIdSceneInfoChild(message, "Head");
                    string handLeftId = JSONParser.GetIdSceneInfoChild(message, "LeftHand");
                    string handRightId = JSONParser.GetIdSceneInfoChild(message, "RightHand");

                    //Force(stream, mainCommand.DeleteNode(handLeftId, "deleteHandL"), "deleteHandL", (message) => Console.WriteLine("Left hand deleted"));
                    //Force(stream, mainCommand.DeleteNode(handRightId, "deleteHandR"), "deleteHandR", (message) => Console.WriteLine("Right hand deleted"));
                });

            //Add route, bike and put camera and bike to follow route at same speed.
            SendMessageAndOnResponse(stream, mainCommand.RouteCommand("routeID"), "routeID", (message) => routeId = JSONParser.GetResponseUuid(message));
            SendMessageAndOnResponse(stream, mainCommand.AddBikeModel("bikeID"), "bikeID",
                (message) =>
                {
                bikeId = JSONParser.GetResponseUuid(message);
                    SendMessageAndOnResponse(stream, mainCommand.addPanel("panelAdd", bikeId), "panelAdd",
                        (message) =>
                            {
                                bool speedReplied = false;
                                bool moveReplied = true;
                                panelId = JSONParser.getPanelID(message);
                                WriteTextMessage(stream, mainCommand.ClearPanel(panelId));
                                

                                SendMessageAndOnResponse(stream, mainCommand.MoveTo(panelId, "panelMove", new float[] { 0f, 0f, 0f }, "Z", 1, 5), "panelMove",
                                    (message) =>
                                    {
                                        Console.WriteLine(message);
                                        SendMessageAndOnResponse(stream, mainCommand.bikeSpeed(panelId, "bikeSpeed", 5.0), "bikeSpeed",
                                            (message) =>
                                                {
                                                    WriteTextMessage(stream, mainCommand.SwapPanel(panelId));
                                                });
                                    });


                                //while (!(speedReplied && moveReplied)) { }
                                
                                while (cameraId == string.Empty) { }
                                SetFollowSpeed(5.0f, stream, mainCommand);
                            });
                });
            
                //Force(stream, mainCommand.addPanel("panelID", bikeId), "panelID",
                //    (message) =>
                //    {
                //        Console.WriteLine("panel response: " + message);
                //        panelId = JSONParser.GetResponseUuid(message);
                //        while(bikeId == string.Empty) { }
                //        SetFollowSpeed(5.0f, stream, mainCommand);
                //    });
                //SendMessageAndOnResponse(stream, maincommand.addpanel("panelid", bikeid), "panelid",
                //    (message) =>
                //    {
                //        console.writeline("panelid: " + message);
                //        //panelid = jsonparser.getpanelid(message);
                //        panelid = jsonparser.getresponseuuid(message);
                //        while (bikeid == string.empty) { }
                //        setfollowspeed(5.0f, stream, maincommand);
                //    });



            //WriteTextMessage(stream, mainCommand.TerrainCommand(new int[] { 256, 256 }, null));
            //string command;



            //Console.WriteLine("id of head " + GetId(Command.STANDARD_HEAD, stream, mainCommand));

            //command = mainCommand.AddModel("car", "data\\customModels\\TeslaRoadster.fbx");
            //WriteTextMessage(stream, command);

            //command = mainCommand.addPanel();
            //  WriteTextMessage(stream, command);
            //  string response = ReadPrefMessage(stream);
            //  Console.WriteLine("add Panel response: \n\r" + response);
            //  string uuidPanel = JSONParser.getPanelID(response);
            //  WriteTextMessage(stream, mainCommand.ClearPanel(uuidPanel));
            //  Console.WriteLine(ReadPrefMessage(stream));
            //  WriteTextMessage(stream, mainCommand.bikeSpeed(uuidPanel, 2.42));
            //  Console.WriteLine(ReadPrefMessage(stream));
            //  WriteTextMessage(stream, mainCommand.ColorPanel(uuidPanel));
            //  Console.WriteLine("Color panel: " + ReadPrefMessage(stream));
            //  WriteTextMessage(stream, mainCommand.SwapPanel(uuidPanel));
            //  Console.WriteLine("Swap panel: " + ReadPrefMessage(stream));
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
            WriteTextMessage(stream, createGraphics.TerrainCommand(new int[] { 256, 256 }, height));

            WriteTextMessage(stream, createGraphics.AddNodeCommand());
        }

        /// <summary>
        /// gets all the children in the current scene
        /// </summary>
        /// <param name="stream">the network stream to send requests to</param>
        /// <param name="createGraphics">the create graphics object to create all the commands</param>
        /// <returns>all the children objects in the current scene</returns>
        public static JArray GetChildren(NetworkStream stream, Command createGraphics)
        {
            JArray res = null;
            SendMessageAndOnResponse(stream, createGraphics.GetSceneInfoCommand("getChildren"), "getChildren", (message) =>
              {
                  dynamic response = JsonConvert.DeserializeObject(message);
                  res = response.data.data.data.children;
              });
            while (res == null) { }
            return res;
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

        private static void SetFollowSpeed(float speed, NetworkStream stream, Command mainCommand)
        {
            WriteTextMessage(stream, mainCommand.RouteFollow(routeId, bikeId, speed, new float[] { 0, -(float)Math.PI / 2f, 0 }, new float[] { 0, 0, 0 }));
            WriteTextMessage(stream, mainCommand.RouteFollow(routeId, cameraId, speed));
            WriteTextMessage(stream, mainCommand.RouteFollow(routeId, panelId, speed, 0, "XYZ", 1, false, new float[] { 0, 0, 0 }, new float[] { 0f, 0f, 150f }));
        }
        //string routeID, string nodeID, float speedValue, float offsetValue, string rotateValue, float smoothingValue, bool followHeightValue, float[] rotateOffsetVector, float[] positionOffsetVector)
        private static void Force(NetworkStream stream, string message, string serial, HandleSerial action)
        {
            SendMessageAndOnResponse(stream, message, serial,
                (message) =>
                {
                    if (!JSONParser.GetStatus(message))
                    {
                        serialResponses.Remove(serial);
                        Force(stream, message, serial,action);
                    } else
                    {
                        action(message);
                    }
                }
                );
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