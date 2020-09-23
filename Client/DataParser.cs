using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    class DataParser
    {
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

        public static bool isRawData(byte[] bytes)
        {
            if (bytes.Length <= 6)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[5] == 0x02;
        }

        private static byte[] getMessage(byte[] payload, byte messageId, byte clientId)
        {
            byte[] res = new byte[payload.Length + 6];

            Array.Copy(BitConverter.GetBytes(payload.Length + 6), 0, res, 0, 4);
            res[4] = messageId;
            Array.Copy(payload, 0, res, 6, payload.Length);

            return res;
        }

        public static byte[] GetRawDataMessage(byte[] payload, byte clientId)
        {
            return getMessage(payload, 0x02, clientId);
        }

        public static byte[] getJsonMessage(byte[] payload, byte clientId)
        {
            return getMessage(payload, 0x01, clientId);
        }

        public static byte[] getJsonMessage(string message, byte clientId)
        {
            return getJsonMessage(Encoding.ASCII.GetBytes(message), clientId);
        }


    }
}
