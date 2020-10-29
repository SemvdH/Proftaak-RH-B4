using LibNoise.Primitive;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RH_Engine
{
    public delegate void HandleSerial(string message);

    public class Program
    {
        private static PC[] PCs = {
            //new PC("DESKTOP-M2CIH87", "Fabian"),
            //new PC("T470S", "Shinichi"),
            //new PC("DESKTOP-DHS478C", "semme"),
            new PC("HP-ZBOOK-SEM", "Sem"),
            //new PC("DESKTOP-TV73FKO", "Wouter"),
            //new PC("DESKTOP-SINMKT1", "Ralf van Aert"),
            //new PC("NA", "Bart")
        };

        private static ServerResponseReader serverResponseReader;
        
        private static string terrainId = string.Empty;
        private static string sessionId = string.Empty;
        private static string tunnelId = string.Empty;
        private static string cameraId = string.Empty;
        private static string routeId = string.Empty;
        private static string panelId = string.Empty;
        private static string bikeId = string.Empty;
        private static string headId = string.Empty;

        private static double bikeSpeed = 6.66;
        private static int bpm = 6;
        private static int power = 6;
        private static int resistance = 6;
        private static string lastMessage = "No message received yet";

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
                if (serialResponses.ContainsKey(serial))
                {
                    serialResponses[serial].Invoke(message);
                    //serialResponses.Remove(serial);
                }
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

            //CreateTerrain(stream, mainCommand);

            //Add route, bike and put camera and bike to follow route at same speed.
            //SendMessageAndOnResponse(stream, mainCommand.RouteCommand("routeID"), "routeID", (message) => routeId = JSONParser.GetResponseUuid(message));
            SendMessageAndOnResponse(stream, mainCommand.AddBikeModelAnim("bikeID",0.01f), "bikeID",
                (message) =>
                {
                bikeId = JSONParser.GetResponseUuid(message);
                    Console.WriteLine("got bike id " + bikeId);
                    SendMessageAndOnResponse(stream, mainCommand.addPanel("panelAdd", bikeId), "panelAdd",
                        (message) =>
                            {
                                bool speedReplied = false;
                                bool moveReplied = true;
                                Console.WriteLine(message);
                                panelId = JSONParser.getPanelID(message);
                                Console.WriteLine("got panel id " + panelId);
                                showPanel(stream, mainCommand);


                                //while (!(speedReplied && moveReplied)) { }
                                
                                //while (cameraId == string.Empty) { }
                                //SetFollowSpeed(5.0f, stream, mainCommand);
                                //WriteTextMessage(stream, mainCommand.RoadCommand(routeId, "road"));
                                //WriteTextMessage(stream, mainCommand.ShowRoute("showRouteFalse", false));
                            });
                });

            //string groundplaneId = GetId("GroundPlane", stream, mainCommand);
            //WriteTextMessage(stream, mainCommand.DeleteNode(groundplaneId, "none"));

            //PlaceHouses(stream, mainCommand);

            //WriteTextMessage(stream, mainCommand.SkyboxCommand(DateTime.Now.Hour));
            
        }

        private static void PlaceHouses(NetworkStream stream, Command mainCommand)
        {
            // public string AddModel(string nodeName, string serial, string fileLocation, float[] positionVector, float scalar, float[] rotationVector)
            //string folderHouses = @"data\NetworkEngine\models\houses\set1\";
            //SendMessageAndOnResponse(stream, mainCommand.AddModel("House1", "uselessSerial_atm", folderHouses + "house1.obj", new float[] { -20f, 1f, 0f }, 4 , new float[] { 0f, 0f, 0f }), "uselessSerial_atm", (message) => Console.WriteLine(message));
            //WriteTextMessage(stream, mainCommand.AddModel("House1", "uselessSerial_atm", @"C:\Users\Ralf van Aert\Documents\AvansTI\NetworkEngine.18.10.10.1\NetworkEngine\data\NetworkEngine\models\houses\set1\house4.obj"));

            PlaceHouse(stream, mainCommand, 2, new float[] { 10f, 1f, 30f }, 1);
            PlaceHouse(stream, mainCommand, 1, new float[] { 42f, 1f, 22f }, new float[] { 0f, 90f, 0f }, 2);
            PlaceHouse(stream, mainCommand, 11, new float[] { -20f, 1f, 0f }, new float[] { 0f, -35f, 0f }, 3);
            PlaceHouse(stream, mainCommand, 7, new float[] { -15f, 1f, 50f }, new float[] { 0f, -50f, 0f }, 4);
            PlaceHouse(stream, mainCommand, 24, new float[] { 40f, 1f, 40f }, new float[] { 0f, 75f, 0f }, 5);
            PlaceHouse(stream, mainCommand, 22, new float[] { 34f, 1f, -20f }, 6);
            PlaceHouse(stream, mainCommand, 14, new float[] { 0f, 1f, -20f }, new float[] { 0f, 210f, 0f }, 7);
        }

        private static void PlaceHouse(NetworkStream stream, Command mainCommand, int numberHousemodel, float[] position, int serialNumber)
        {
            PlaceHouse(stream, mainCommand, numberHousemodel, position, new float[] { 0f, 0f, 0f }, serialNumber);
        }

        private static void PlaceHouse(NetworkStream stream, Command mainCommand, int numberHousemodel, float[] position, float[] rotation, int serialNumber)
        {
            string folderHouses = @"data\NetworkEngine\models\houses\set1\";
            SendMessageAndOnResponse(stream, mainCommand.AddModel("House1", "housePlacement" + serialNumber, folderHouses + "house" + numberHousemodel + ".obj", position, 4, rotation), "housePlacement" + serialNumber, (message) => Console.WriteLine(message));
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
                height[i] = improvedPerlin.GetValue(x / 10, x / 10, x * 100)/3.5f + 1;
                
                if (height[i] > 1.1f)
                {
                    height[i] = height[i] * 0.8f;
                }
                else if (height[i] < 0.9f)
                {
                    height[i] = height[i] * 1.2f;
                }
                x += 0.001f;
            }

            SendMessageAndOnResponse(stream, createGraphics.TerrainAdd(new int[] { 256, 256 }, height, "terrain"), "terrain",
                (message) =>
                {

                    SendMessageAndOnResponse(stream, createGraphics.renderTerrain("renderTerrain"), "renderTerrain",
                        (message) =>
                        {
                            terrainId = JSONParser.GetTerrainID(message);
                            string addLayerMsg = createGraphics.AddLayer(terrainId, "addLayer");
                        SendMessageAndOnResponse(stream, addLayerMsg, "addLayer", (message) => Console.WriteLine(""));

                        });
                });
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

        
        private static void showPanel(NetworkStream stream, Command mainCommand)
        {
            //WriteTextMessage(stream, mainCommand.ColorPanel(panelId));
            WriteTextMessage(stream, mainCommand.ClearPanel(panelId));
            SendMessageAndOnResponse(stream, mainCommand.showBikespeed(panelId, "bikeSpeed", bikeSpeed), "bikeSpeed",
                (message) =>
                {
                    Console.WriteLine(message);
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(stream, mainCommand.showHeartrate(panelId, "bpm", bpm), "bpm",
                (message) =>
                {
                    Console.WriteLine(message);
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(stream, mainCommand.showPower(panelId, "power", power), "power",
                (message) =>
                {
                    Console.WriteLine(message);
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(stream, mainCommand.showResistance(panelId, "resistance", resistance), "resistance",
                (message) =>
                {
                    Console.WriteLine(message);
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(stream, mainCommand.showMessage(panelId, "message", lastMessage), "message",
                (message) =>
                {
                    Console.WriteLine(message);
                    // TODO check if is drawn
                });

            // Check if every text is drawn!

            WriteTextMessage(stream, mainCommand.SwapPanel(panelId));
        }

        private static void SetFollowSpeed(float speed, NetworkStream stream, Command mainCommand)
        {
            WriteTextMessage(stream, mainCommand.RouteFollow(routeId, bikeId, speed, new float[] { 0, -(float)Math.PI / 2f, 0 }, new float[] { 0, 0, 0 }));
            WriteTextMessage(stream, mainCommand.RouteFollow(routeId, cameraId, speed));
            //WriteTextMessage(stream, mainCommand.RouteFollow(routeId, panelId, speed, 1f, "XYZ", 1, false, new float[] { 0, 0, 0 }, new float[] { 0f, 5f, 5f }));
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