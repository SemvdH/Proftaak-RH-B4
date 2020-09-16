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

        public string TerrainCommand()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue("tunnel/send");
                writer.WritePropertyName("data");
                writer.WriteStartObject();
                writer.WritePropertyName("dest");
                writer.WriteValue(tunnelID);
                writer.WritePropertyName("data");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue("scene/terrain/add");
                writer.WritePropertyName("data");
                writer.WriteStartObject();
                writer.WritePropertyName("size");
                writer.WriteStartArray();
                writer.WriteValue(2);
                writer.WriteValue(2);
                writer.WriteEndArray();
                writer.WritePropertyName("heights");
                writer.WriteStartArray();
                writer.WriteValue(30000000);
                writer.WriteValue(2);
                writer.WriteValue(4);
                writer.WriteValue(7);
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndObject();

            }

            Console.WriteLine(sb.ToString());
            return sb.ToString();
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

            dynamic packet = Payload(payload);
            Console.WriteLine(JsonConvert.SerializeObject(packet));
            return JsonConvert.SerializeObject(packet);

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
