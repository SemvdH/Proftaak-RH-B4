using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            return AddModel(nodeName, fileLocation, null, new float[]{ 0,0,0} ,1, new float[] { 0, 0, 0 });
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


        public string RouteCommand()
        {
            return "";
        }

        public string FollowRouteCommand()
        {
            return "";
        }

        public string RoadCommand()
        {
            return "";
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
