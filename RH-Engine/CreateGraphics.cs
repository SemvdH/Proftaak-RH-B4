using LibNoise.Primitive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RH_Engine
{
    class CreateGraphics
    {
        public const string STANDARD_HEAD = "Head";
        public const string STANDARD_GROUND = "GroundPlane";
        public const string STANDARD_SUN = "SunLight";
        public const string STANDARD_LEFTHAND = "LeftHand";
        public const string STANDARD_RIGHTHAND = "RightHand";



        string tunnelID;

        public CreateGraphics(string tunnelID)
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

        public string AddBikeModel()
        {
            return AddModel("bike", "data\\NetworkEngine\\models\\bike\\bike.fbx");
        }

        public string AddModel(string nodeName, string fileLocation)
        {
            return AddModel(nodeName, fileLocation, null, new float[] { 0, 0, 0 }, 1, new float[] { 0, 0, 0 });
        }

        public string AddModel(string nodeName, string fileLocation, float[] positionVector, float scalar, float[] rotationVector)
        {
            return AddModel(nodeName, fileLocation, null, positionVector, scalar, rotationVector);
        }

        public string AddModel(string nodeName, string fileLocation, string animationLocation, float[] positionVector, float scalar, float[] rotationVector)
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

        public string MoveTo(string uuid, float[] positionVector, string rotateValue, float speedValue, float timeValue)
        {
            return MoveTo(uuid, "idk", positionVector, rotateValue, "linear", false, speedValue, timeValue);
        }

        private string MoveTo(string uuid, string stopValue, float[] positionVector, string rotateValue, string interpolateValue, bool followHeightValue, float speedValue, float timeValue)
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

        public string RouteCommand()
        {
            ImprovedPerlin improvedPerlin = new ImprovedPerlin(4325, LibNoise.NoiseQuality.Best);
            Random r = new Random();
            dynamic payload = new
            {
                id = "route/add",
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

        public string RouteFollow(string routeID, string nodeID, float speedValue)
        {
            return RouteFollow(routeID, nodeID, speedValue, new float[] { 0, 0, 0 });
        }

        public string RouteFollow(string routeID, string nodeID, float speedValue, float[] positionOffsetVector)
        {
            return RouteFollow(routeID, nodeID, speedValue, 0, "XYZ", 1, true, new float[] { 0, 0, 0 }, positionOffsetVector);
        }
        private string RouteFollow(string routeID, string nodeID, float speedValue, float offsetValue, string rotateValue, float smoothingValue, bool followHeightValue, float[] rotateOffsetVector, float[] positionOffsetVector)
        {
            dynamic payload = new
            {
                id = "route/follow",
                data = new
                {
                    route = routeID,
                    node = nodeID,
                    speed = speedValue,
                    offset = offsetValue,
                    rotate = rotateValue,
                    smoothing = smoothingValue,
                    followHeight = followHeightValue,
                    rotateOffset = rotateOffsetVector,
                    positionOffset = positionOffsetVector

                }
            };
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
            int[] dir = { rng.Next(50), 0, rng.Next(50) };
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

        public string GetSceneInfoCommand()
        {
            dynamic payload = new
            {
                id = "scene/get"
            };

            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string ResetScene()
        {
            dynamic payload = new
            {
                id = "scene/reset"
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
