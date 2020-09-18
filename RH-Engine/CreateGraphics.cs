using Newtonsoft.Json;
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
