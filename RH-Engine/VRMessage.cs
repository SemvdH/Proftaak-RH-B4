using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RH_Engine
{
    class VRMessage
    {
        public VRMessage(string id, params JObject[] data)
        {
            this.Id = id;
            this.Data = data;
        }

        public string Id 
        {
            get; set; 
        }

        public JObject[] Data
        {
            get;set;
        }

        public string GetCommand()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(this.Id);
                writer.WritePropertyName("data");
                writer.WriteStartArray();
                foreach (JObject o in Data)
                {
                    writer.WriteValue(o);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            return sb.ToString();

        }
    }
}
