using System;
using System.Collections.Generic;
using System.Text;
using RH_Engine;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LibNoise.Primitive;

namespace ClientApp.Utils
{
    public delegate void HandleSerial(string message);
    public delegate void EngineDelegate();

    public sealed class EngineConnection
    {
        private static EngineConnection instance = null;
        private static readonly object padlock = new object();
        private static System.Timers.Timer updateTimer;
        private static System.Timers.Timer noVRResponseTimer;
        public EngineDelegate OnNoTunnelId;
        public EngineDelegate OnSuccessFullConnection;
        public EngineDelegate OnEngineDisconnect;


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
        private static string cameraId = string.Empty;
        private static string routeId = string.Empty;
        private static string panelId = string.Empty;
        private static string bikeId = string.Empty;
        private static string headId = string.Empty;
        private static string groundPlaneId = string.Empty;
        private static string terrainId = string.Empty;
        private static string lastMessage = "No message received yet";

        public float BikeSpeed { get; set; }
        public float BikePower { get; set; }
        public float BikeBPM { get; set; }
        public float BikeResistance { get; set; }

        public bool FollowingRoute = false;

        private static NetworkStream stream;

        private static Dictionary<string, HandleSerial> serialResponses = new Dictionary<string, HandleSerial>();
        private Command mainCommand;

        public bool Connected = false;

        EngineConnection()
        {
            BikeSpeed = 0;
            BikePower = 0;
            BikeBPM = 0;
            BikeResistance = 50;
            updateTimer = new System.Timers.Timer(1000);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = false;

            noVRResponseTimer = new System.Timers.Timer(15000);
            noVRResponseTimer.Elapsed += noVRResponseTimeout;
            noVRResponseTimer.AutoReset = false;
            noVRResponseTimer.Enabled = false;
            
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateInfoPanel();
        }

        private void noVRResponseTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            Write("VR RESPONSE TIMEOUT");
            noVRResponseTimer.Stop();
            sessionId = string.Empty;
            tunnelId = string.Empty;
            cameraId = string.Empty;
            routeId = string.Empty;
            panelId = string.Empty;
            bikeId = string.Empty;
            groundPlaneId = string.Empty;
            terrainId = string.Empty;
            OnEngineDisconnect?.Invoke();
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
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



        /// <summary>
        /// connects to the vr engine and initalizes the serverResponseReader
        /// </summary>
        public void Connect()
        {
            TcpClient client = new TcpClient("145.48.6.10", 6666);
            stream = client.GetStream();
            initReader();
            CreateConnection();
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
        }

        #region VR Message traffic
        /// <summary>
        /// connects to the server and creates the tunnel
        /// </summary>
        /// <param name="stream">the network stream to use</param>
        public void CreateConnection()
        {

            WriteTextMessage("{\r\n\"id\" : \"session/list\",\r\n\"serial\" : \"list\"\r\n}");

            // wait until we have got a sessionId
            while (sessionId == string.Empty) { }

            string tunnelCreate = "{\"id\" : \"tunnel/create\",	\"data\" :	{\"session\" : \"" + sessionId + "\"}}";

            WriteTextMessage(tunnelCreate);


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
                Console.WriteLine("set tunnel id to " + tunnelId);
                if (tunnelId == null)
                {
                    Write("could not find a valid tunnel id!");
                    OnNoTunnelId?.Invoke();
                    Connected = false;
                    FollowingRoute = false;
                    return;
                }
                else
                {
                    Write("got tunnel id! " + tunnelId);
                    Connected = true;
                    noVRResponseTimer.Enabled = true;
                    OnSuccessFullConnection?.Invoke();
                }
            }

            if (message.Contains("serial"))
            {
                //Console.WriteLine("GOT MESSAGE WITH SERIAL: " + message + "\n\n\n");
                string serial = JSONParser.GetSerial(message);
                //Console.WriteLine("Got serial " + serial);
                if (serialResponses.ContainsKey(serial)) serialResponses[serial].Invoke(message);
            }

            noVRResponseTimer.Stop();
            noVRResponseTimer.Start();
        }

        public void initScene()
        {

            Write("initializing scene...");
            mainCommand = new Command(tunnelId);

            // reset the scene
            WriteTextMessage(mainCommand.ResetScene());

            //Get sceneinfo and set the id's
            SendMessageAndOnResponse(mainCommand.GetSceneInfoCommand("sceneinfo"), "sceneinfo",
                (message) =>
                {
                    //Console.WriteLine("\r\n\r\n\r\nscene info" + message);
                    cameraId = JSONParser.GetIdSceneInfoChild(message, "Camera");
                    string headId = JSONParser.GetIdSceneInfoChild(message, "Head");
                    string handLeftId = JSONParser.GetIdSceneInfoChild(message, "LeftHand");
                    string handRightId = JSONParser.GetIdSceneInfoChild(message, "RightHand");
                    groundPlaneId = JSONParser.GetIdSceneInfoChild(message, "GroundPlane");
                    Write("--- Ground plane id is " + groundPlaneId);
                });
            // add the route and set the route id
            CreateTerrain();
            SendMessageAndOnResponse(mainCommand.RouteCommand("routeID"), "routeID", (message) => routeId = JSONParser.GetResponseUuid(message));
        }

        internal void StartRouteFollow()
        {
            Write("Starting route follow...");
            FollowingRoute = true;

            SendMessageAndOnResponse(mainCommand.AddBikeModel("bikeID"), "bikeID",
                (message) =>
                {
                    bikeId = JSONParser.GetResponseUuid(message);
                    SendMessageAndOnResponse(mainCommand.addPanel("panelAdd", bikeId), "panelAdd",
                        (message) =>
                        {

                            panelId = JSONParser.getPanelID(message);

                            WriteTextMessage(mainCommand.ColorPanel(panelId));
                            UpdateInfoPanel();
                            updateTimer.Enabled = true;

                            while (cameraId == string.Empty) { }
                            SetFollowSpeed(5.0f);
                            WriteTextMessage(mainCommand.RoadCommand(routeId, "road"));
                            WriteTextMessage(mainCommand.ShowRoute("showRouteFalse", false));
                        });
                });
                setEnvironment();

            
        }

        private void setEnvironment()
        {
            Write("Setting environment");
            WriteTextMessage(mainCommand.DeleteNode(groundPlaneId, "none"));

            PlaceHouses();

            WriteTextMessage(mainCommand.SkyboxCommand(DateTime.Now.Hour));
        }

        private void PlaceHouses()
        {
            PlaceHouse(2, new float[] { 10f, 1f, 30f }, 1);
            PlaceHouse(1, new float[] { 42f, 1f, 22f }, new float[] { 0f, 90f, 0f }, 2);
            PlaceHouse(11, new float[] { -20f, 1f, 0f }, new float[] { 0f, -35f, 0f }, 3);
            PlaceHouse(7, new float[] { -15f, 1f, 50f }, new float[] { 0f, -50f, 0f }, 4);
            PlaceHouse(24, new float[] { 40f, 1f, 40f }, new float[] { 0f, 75f, 0f }, 5);
            PlaceHouse(22, new float[] { 34f, 1f, -20f }, 6);
            PlaceHouse(14, new float[] { 0f, 1f, -20f }, new float[] { 0f, 210f, 0f }, 7);
        }

        private void PlaceHouse(int numberHousemodel, float[] position, int serialNumber)
        {
            PlaceHouse(numberHousemodel, position, new float[] { 0f, 0f, 0f }, serialNumber);
        }

        private void PlaceHouse(int numberHousemodel, float[] position, float[] rotation, int serialNumber)
        {
            string folderHouses = @"data\NetworkEngine\models\houses\set1\";
            SendMessageAndOnResponse(mainCommand.AddModel("House1", "housePlacement" + serialNumber, folderHouses + "house" + numberHousemodel + ".obj", position, 4, rotation), "housePlacement" + serialNumber, (message) => Console.WriteLine(message));
        }

        public void UpdateInfoPanel()
        {
            ShowPanel(BikeSpeed, (int)BikeBPM, (int)BikePower, (int)BikeResistance);
            WriteTextMessage(mainCommand.RouteSpeed(BikeSpeed, bikeId));
            WriteTextMessage(mainCommand.RouteSpeed(BikeSpeed, cameraId));
        }

        public void ShowPanel(double bikeSpeed, int bpm, int power, int resistance)
        {
            WriteTextMessage(mainCommand.ClearPanel(panelId));

            SendMessageAndOnResponse(mainCommand.showBikespeed(panelId, "bikeSpeed", bikeSpeed), "bikeSpeed",
                (message) =>
                {
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(mainCommand.showHeartrate(panelId, "bpm", bpm), "bpm",
                (message) =>
                {
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(mainCommand.showPower(panelId, "power", power), "power",
                (message) =>
                {
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(mainCommand.showResistance(panelId, "resistance", resistance), "resistance",
                (message) =>
                {
                    // TODO check if is drawn
                });
            SendMessageAndOnResponse(mainCommand.showMessage(panelId, "message", lastMessage), "message",
                (message) =>
                {
                    // TODO check if is drawn
                });

            // Check if every text is drawn!

            WriteTextMessage(mainCommand.SwapPanel(panelId));
        }

        private void SetFollowSpeed(float speed)
        {
            WriteTextMessage(mainCommand.RouteFollow(routeId, bikeId, speed, new float[] { 0, -(float)Math.PI / 2f, 0 }, new float[] { 0, 0, 0 }));
            WriteTextMessage(mainCommand.RouteFollow(routeId, cameraId, speed));
        }

        public void CreateTerrain()
        {
            float x = 0f;
            float[] height = new float[256 * 256];
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(0, LibNoise.NoiseQuality.Best);
            for (int i = 0; i < 256 * 256; i++)
            {
                height[i] = improvedPerlin.GetValue(x /10, x / 10, x * 100) / 3.5f + 1;

                //if (height[i] > 1.1f)
                //{
                //    height[i] = height[i] * 0.8f;
                //}
                //else if (height[i] < 0.9f)
                //{
                //    height[i] = height[i] * 1.2f;
                //}
                x += 0.001f;
            }

            SendMessageAndOnResponse(mainCommand.TerrainAdd(new int[] { 256, 256 }, height, "terrain"), "terrain",
                (message) =>
                {

                    SendMessageAndOnResponse(mainCommand.renderTerrain("renderTerrain"), "renderTerrain",
                        (message) =>
                        {
                            terrainId = JSONParser.GetTerrainID(message);
                            string addLayerMsg = mainCommand.AddLayer(terrainId, "addLayer");
                            SendMessageAndOnResponse(addLayerMsg, "addLayer", (message) => Console.WriteLine(""));

                        });
                });
        }

        #endregion

        #region message send/receive

        /// <summary>
        /// method that sends the speciefied message with the specified serial, and executes the given action upon receivind a reply from the server with this serial.
        /// </summary>
        /// <param name="stream">the networkstream to use</param>
        /// <param name="message">the message to send</param>
        /// <param name="serial">the serial to check for</param>
        /// <param name="action">the code to be executed upon reveiving a reply from the server with the specified serial</param>
        public void SendMessageAndOnResponse(string message, string serial, HandleSerial action)
        {
            if (serialResponses.ContainsKey(serial))
            {
                serialResponses[serial] = action;
            }
            else
            {
                serialResponses.Add(serial, action);
            }
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

            //Write("sent message " + message);
        }

        #endregion

        public void Stop()
        {
            serverResponseReader.Stop();

        }
        public void Write(string msg)
        {
            Debug.WriteLine("[ENGINECONNECT] " + msg);
        }

    }


}
