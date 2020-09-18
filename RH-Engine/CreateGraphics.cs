﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RH_Engine
{
    class CreateGraphics
    {
        string tunnelID;

        public CreateGraphics(string tunnelID)
        {
            this.tunnelID = tunnelID;
        }

        public string TerrainCommand(int[] sizeAray, int[] heightsArray)
        {
            dynamic payload = new
            {
                id = "scene/terrain/add",
                data = new
                {
                    size = sizeAray,
                    heights = heightsArray
                }

            };
            return JsonConvert.SerializeObject(Payload(payload));
        }

        public string DeleteGroundPaneCommand(string uuid)
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

        public string ModelCommand()
        {
            return "";
        }

        public string RouteCommand()
        {
            dynamic payload = new
            {
                id = "route/add",
                data = new
                {
                    nodes = new dynamic[]
                    {
                        pos = new int[]{ 0,0,0},
                        dir = new int[]{ 5,0,-5}
                    }
                }
            };
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
                data = new {
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
