using Newtonsoft.Json;
using System;

namespace Message
{
    /// <summary>
    /// Message class to handle traffic between clients and server
    /// </summary>
    public class Message
    {

        public static void Main(string[] args)
        {

        }
        /// <summary>
        /// identifier for the message
        /// </summary>
        public Identifier Identifier
        {
            get;set;
        }

        /// <summary>
        /// payload of the message, the actual text
        /// </summary>
        public string Payload
        {
            get;set;
        }

        /// <summary>
        /// constructs a new message with the given parameters
        /// </summary>
        /// <param name="identifier">the identifier</param>
        /// <param name="payload">the payload</param>
        public Message(Identifier identifier, string payload)
        {
            this.Identifier = identifier;
            this.Payload = payload;
        }

        /// <summary>
        /// serializes this object to a JSON string
        /// </summary>
        /// <returns>a JSON representation of this object</returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// deserializes a JSON string into a new Message object
        /// </summary>
        /// <param name="json">the JSON string to deserialize</param>
        /// <returns>a new <c>Message</c> object from the JSON string</returns>
        public static Message Deserialize(string json)
        {
            return (Message)JsonConvert.DeserializeObject(json);
        }
    }

    /// <summary>
    /// Identifier enum for the Message objects
    /// </summary>
    public enum Identifier
    {
        LOGIN,
        CHAT,
    }
}
