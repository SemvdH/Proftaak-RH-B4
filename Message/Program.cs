using Newtonsoft.Json;
using System;

namespace Message
{
    public class Message
    {
        public string Identifier
        {
            get;set;
        }

        public string Payload
        {
            get;set;
        }

        public Message(string identifier, string payload)
        {
            this.Identifier = identifier;
            this.Payload = payload;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Message Deserialize(string json)
        {
            return (Message)JsonConvert.DeserializeObject(json);
        }
    }
}
