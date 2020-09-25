using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Client
{
    public class DataParser
    {
        /// <summary>
        /// makes the json object with LOGIN identifier and username and password
        /// </summary>
        /// <param name="mUsername">username</param>
        /// <param name="mPassword">password</param>
        /// <returns>json object to ASCII to bytes</returns>
        public static byte[] GetLoginJson(string mUsername, string mPassword)
        {
            dynamic json = new
            {
                identifier = "LOGIN",
                data = new
                {
                    username = mUsername,
                    password = mPassword,
                }
            };

            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json));
        }

        /// <summary>
        /// get the identifier from json
        /// </summary>
        /// <param name="bytes">json in ASCII</param>
        /// <param name="identifier">gets the identifier</param>
        /// <returns>if it sucseeded</returns>
        public static bool getJsonIdentifier(byte[] bytes, out string identifier)
        {
            if (bytes.Length <= 6)
            {
                throw new ArgumentException("bytes to short");
            }
            byte messageId = bytes[4];

            if (messageId == 1)
            {
                dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(bytes.Skip(6).ToArray()));
                identifier = json.identifier;
                return true;
            }
            else
            {
                identifier = "";
                return false;
            }
        }

        /// <summary>
        /// checks if the de message is raw data according to the protocol
        /// </summary>
        /// <param name="bytes">message</param>
        /// <returns>if message contains raw data</returns>
        public static bool isRawData(byte[] bytes)
        {
            if (bytes.Length <= 6)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[4] == 0x02;
        }

        /// <summary>
        /// constructs a message with the payload, messageId and clientId
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="messageId"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getMessage(byte[] payload, byte messageId, byte clientId)
        {
            byte[] res = new byte[payload.Length + 6];

            Array.Copy(BitConverter.GetBytes(payload.Length + 6), 0, res, 0, 4);
            res[4] = messageId;
            res[5] = clientId;
            Array.Copy(payload, 0, res, 6, payload.Length);

            return res;
        }

        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is raw data
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] GetRawDataMessage(byte[] payload, byte clientId)
        {
            return getMessage(payload, 0x02, clientId);
        }

        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is json
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getJsonMessage(byte[] payload, byte clientId)
        {
            return getMessage(payload, 0x01, clientId);
        }

        /// <summary>
        /// constructs a message with the message and clientId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getJsonMessage(string message, byte clientId)
        {
            return getJsonMessage(Encoding.ASCII.GetBytes(message), clientId);
        }


    }
}
