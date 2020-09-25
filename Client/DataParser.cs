using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Client
{
    public class DataParser
    {
        public const string LOGIN = "LOGIN";
        public const string LOGIN_RESPONSE = "LOGIN_RESPONSE";
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
                identifier = LOGIN,
                data = new
                {
                    username = mUsername,
                    password = mPassword,
                }
            };

            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json));
        }

        public static bool GetUsernamePassword(byte[] jsonbytes, out string username, out string password)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(jsonbytes));
            try
            {
                username = json.data.username;
                password = json.data.password;
                return true;
            }
            catch
            {
                username = null;
                password = null;
                return false;
            }
        }

        private static byte[] getJsonMessage(string mIdentifier, dynamic data)
        {
            dynamic json = new
            {
                identifier = mIdentifier,
                data
            };
            return getMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(json)), 0x01);
        }

        public static byte[] getLoginResponse(string mStatus)
        {
            return getJsonMessage(LOGIN_RESPONSE, new { status = mStatus });
        }

        public static string getResponseStatus(byte[] json)
        {
            return ((dynamic)JsonConvert.DeserializeObject(Encoding.ASCII.GetString(json))).data.status;
        }

        /// <summary>
        /// get the identifier from json
        /// </summary>
        /// <param name="bytes">json in ASCII</param>
        /// <param name="identifier">gets the identifier</param>
        /// <returns>if it sucseeded</returns>
        public static bool getJsonIdentifier(byte[] bytes, out string identifier)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            byte messageId = bytes[4];

            if (messageId == 0x01)
            {
                dynamic json = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(bytes.Skip(5).ToArray()));
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
            if (bytes.Length <= 5)
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
        private static byte[] getMessage(byte[] payload, byte messageId)
        {
            byte[] res = new byte[payload.Length + 5];

            Array.Copy(BitConverter.GetBytes(payload.Length + 5), 0, res, 0, 4);
            res[4] = messageId;
            Array.Copy(payload, 0, res, 5, payload.Length);

            return res;
        }

        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is raw data
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] GetRawDataMessage(byte[] payload)
        {
            return getMessage(payload, 0x02);
        }

        /// <summary>
        /// constructs a message with the payload and clientId and assumes the payload is json
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getJsonMessage(byte[] payload)
        {
            return getMessage(payload, 0x01);
        }

        /// <summary>
        /// constructs a message with the message and clientId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientId"></param>
        /// <returns>the message ready for sending</returns>
        public static byte[] getJsonMessage(string message)
        {
            return getJsonMessage(Encoding.ASCII.GetBytes(message));
        }


    }
}
