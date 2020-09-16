using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RH_Engine
{
    class CreateGraphics
    {
        string sessionID;

        public CreateGraphics(string sessionID)
        {
            this.sessionID = sessionID;
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
                writer.WriteValue("TODO");
                writer.WritePropertyName("data");
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue("scene/terrain/add");
                writer.WritePropertyName("data");
                writer.WriteStartObject();
                writer.WritePropertyName("size");
                writer.WriteValue("[2,2]");
                writer.WritePropertyName("heights");
                writer.WriteValue("[0,0,0,0]");
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndObject();

            }

            Console.WriteLine(sb.ToString());
            return sb.ToString();
        }
    }
}
