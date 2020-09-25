using LibNoise.Primitive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RH_Engine
{
    class Command
    {
        public const string STANDARD_HEAD = "Head";
        public const string STANDARD_GROUND = "GroundPlane";
        public const string STANDARD_SUN = "SunLight";
        public const string STANDARD_LEFTHAND = "LeftHand";
        public const string STANDARD_RIGHTHAND = "RightHand";



        string tunnelID;

        public Command(string tunnelID)
        {
            this.tunnelID = tunnelID;
        }

        public string TerrainCommand(int[] sizeArray, float[] heightsArray)
        {
            dynamic payload = new
            {
                id = "scene/terrain/add",
                data = new
                {
                    size = sizeArray,
                    heights = heightsArray
                }

            };
            return JsonConvert.SerializeObject(Payload(payload));
        }
        public string AddLayer(string uid, string texture)
        {
            dynamic payload = new
            {
                id = "scene/node/addlayer",
                data = new
                {
                    id = uid,
                    diffuse = @"C:\Users\woute\Downloads\NetworkEngine.18.10.10.1\NetworkEngine\data\NetworkEngine\textures\terrain\adesert_cracks_d.jpg",
                    normal = @"C:\Users\woute\Downloads\NetworkEngine.18.10.10.1\NetworkEngine\data\NetworkEngine\textures\terrain\adesert_mntn_d.jpg",
                    minHeight = 0,
                    maxHeight = 10,
                    fadeDist = 1
                }
            };
            return JsonConvert.SerializeObject(Payload(payload));
        }
        public string UpdateTerrain()
        {
            dynamic payload = new
            {
                id = "scene/terrain/update",
                data = new
                {

                }
            };
            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string AddNodeCommand()
        {
            dynamic payload = new
            {
                id = "scene/node/add",
                data = new
                {
                    name = "newNode",
                    components = new
                    {
                        terrain = new
                        {
                            smoothnormals = true
                        }
                    }
                }
            };
            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string DeleteNode(string uuid)
        {

            dynamic payload = new
            {
                id = "scene/node/delete",
                data = new
                {
                    id = uuid,

                }

            };
            return JsonConvert.SerializeObject(Payload(payload));

        }

        public string addPanel(string serialToSend)
        {
            dynamic payload = new
            {
                id = "scene/node/add",
                serial = serialToSend,
                data = new
                {
                    name = "dashboard",
                    components = new
                    {
                        panel = new
                        {
                            size = new int[] { 1, 1 },
                            resolution = new int[] { 512, 512 },
                            background = new int[] { 1, 0, 0, 0 },
                            castShadow = false

                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string ColorPanel(string uuidPanel)
        {
            dynamic payload = new
            {
                id = "scene/panel/setclearcolor",
                data = new
                {
                    id = uuidPanel,
                    color = new int[] { 1, 1, 1, 1 }
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string SwapPanel(string uuid)
        {
            dynamic payload = new
            {
                id = "scene/panel/swap",
                data = new
                {
                    id = uuid
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string bikeSpeed(string uuidPanel, double speed)
        {
            dynamic payload = new
            {
                id = "scene/panel/drawtext",
                data = new
                {
                    id = uuidPanel,
                    text = "Bike speed placeholder",
                    position = new int[] { 0, 0 },
                    size = 32.0,
                    color = new int[] { 0, 0, 0, 1 },
                    font = "segoeui"
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string SwapPanelCommand(string uuid)
        {
            dynamic payload = new
            {
                id = "scene/panel/swap",
                data = new
                {
                    id = uuid
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string ClearPanel(string uuid)
        {
            dynamic payload = new
            {
                id = "scene/panel/clear",
                data = new
                {
                    id = uuid
                }
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string AddBikeModel()
        {
            return AddModel("bike","addbike", "data\\NetworkEngine\\models\\bike\\bike.fbx");
        }

        public string AddModel(string nodeName,string serial, string fileLocation)
        {
            return AddModel(nodeName,serial, fileLocation, null, new float[] { 0, 0, 0 }, 1, new float[] { 0, 0, 0 });
        }

        public string AddModel(string nodeName,string serial, string fileLocation, float[] positionVector, float scalar, float[] rotationVector)
        {
            return AddModel(nodeName,serial, fileLocation, null, positionVector, scalar, rotationVector);
        }

        public string AddModel(string nodeName,string serialToSend, string fileLocation, string animationLocation, float[] positionVector, float scalar, float[] rotationVector)
        {
            string namename = nodeName;
            bool animatedBool = false;
            if (animationLocation != null)
            {
                animatedBool = true;
            }

            dynamic payload = new
            {
                id = "scene/node/add",
                serial = serialToSend,
                data = new
                {
                    name = namename,
                    components = new
                    {
                        transform = new
                        {
                            position = positionVector,
                            scale = scalar,
                            rotation = rotationVector

                        },
                        model = new
                        {
                            file = fileLocation,
                            cullbackfaces = true,
                            animated = animatedBool,
                            animation = animationLocation
                        },
                    }
                }

            };
            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string MoveTo(string uuid, float[] positionVector, float rotateValue, float speedValue, float timeValue)
        {
            return MoveTo(uuid, "idk", positionVector, rotateValue, "linear", false, speedValue, timeValue);
        }

        private string MoveTo(string uuid, string stopValue, float[] positionVector, float rotateValue, string interpolateValue, bool followHeightValue, float speedValue, float timeValue)
        {
            dynamic payload = new
            {
                id = "scene/node/moveto",
                data = new
                {
                    id = uuid,
                    stop = stopValue,
                    position = positionVector,
                    rotate = rotateValue,
                    interpolate = interpolateValue,
                    followheight = followHeightValue,
                    speed = speedValue,
                    time = timeValue
                }
            };
            return JsonConvert.SerializeObject(Payload(payload));
        }


        public string RouteCommand(string serialToSend)
        {
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(4325, LibNoise.NoiseQuality.Best);
            Random r = new Random();
            dynamic payload = new
            {
                id = "route/add",
                serial = serialToSend,
                data = new
                {
                    nodes = new dynamic[]
                    {
                        new
                        {
                            /*pos = GetPos(0.6f, improvedPerlin)*/
                            pos = new int[] {0,0,5 },
                            dir = new int[] { r.Next(20,100),0,-r.Next(20, 100) }
                        },
                        new
                        {
                            //pos = GetPos(1.6f, improvedPerlin),
                            pos = new int[] {50,0,0 },
                             dir = new int[] { r.Next(20, 100),0,r.Next(20, 100) }
                        },
                        new
                        {
                            //pos = GetPos(2.654f, improvedPerlin),
                             pos = new int[] {20,0,20 },
                             dir = new int[] { r.Next(20, 100),0,r.Next(20, 100) }
                        },
                        new
                        {
                            //pos = GetPos(3.6543f, improvedPerlin),
                             pos = new int[] {10,0,50 },
                             dir = new int[] { -r.Next(3,7),0,r.Next(3,7) }
                        },
                        new
                        {
                            pos = new int[] {0,0,50 },
                             dir = new int[] { -r.Next(20, 50),0,-r.Next(20, 50) }
                        }
                    }
                }
            };
            Console.WriteLine("route command: " + JsonConvert.SerializeObject(Payload(payload)));
            return JsonConvert.SerializeObject(Payload(payload));
        }

        private float[] GetPos(float n, ImprovedPerlin improvedPerlin)
        {
            float[] res = new float[] { improvedPerlin.GetValue(n) * 50, 0, improvedPerlin.GetValue(n) * 50 };
            return res;
        }

        private int[] GetDir()
        {
            Random rng = new Random();
            int[] dir = {rng.Next(50), 0, rng.Next(50)};
            return dir;
        }

        public string FollowRouteCommand()
        {
            return "";
        }

        public string RoadCommand(string uuid_route)
        {
            Console.WriteLine("road");
            dynamic payload = new
            {
                id = "scene/road/add",
                data = new
                {
                    route = uuid_route,
                    diffuse = "data/NetworkEngine/textures/tarmac_diffuse.png",
                    normal = "data/NetworkEngine/textures/tarmac_normale.png",
                    specular = "data/NetworkEngine/textures/tarmac_specular.png",
                    heightoffset = 1f
                }
            };
            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string GetSceneInfoCommand(string serialToSend)
        {
            dynamic payload = new
            {
                id = "scene/get",
                serial = serialToSend
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string ResetScene()
        {
            dynamic payload = new
            {
                id = "scene/reset",
                serial = "reset"
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string SkyboxCommand(double timeToSet)
        {
            if (timeToSet < 0 || timeToSet > 24)
            {
                throw new Exception("The time must be between 0 and 24!");
            }


            dynamic payload = new
            {
                id = "scene/skybox/settime",
                data = new
                {
                    time = timeToSet
                }

            };
            return JsonConvert.SerializeObject(Payload(payload));

        }


        private object Payload(dynamic message)
        {
            return new
            {
                id = "tunnel/send",
                data = new
                {
                    dest = tunnelID,
                    data = message,
                }
            };
        }



    }
}
