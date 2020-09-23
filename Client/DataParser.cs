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
    public class DataParser
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
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            byte messageId = bytes[4];

            if (messageId == 1)
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

        public static bool isRawData(byte[] bytes)
        {
            if (bytes.Length <= 5)
            {
                throw new ArgumentException("bytes to short");
            }
            return bytes[5] == 0x02;
        }

        public static byte[] getMessage(byte[] payload, byte messageId)
        {
            byte[] res = new byte[payload.Length + 6];

            Array.Copy(BitConverter.GetBytes(payload.Length + 6), 0, res, 0, 4);
            res[4] = messageId;
            Array.Copy(payload, 0, res, 6, payload.Length);

            return res;
        }

        public static byte[] GetRawDataMessage(byte[] payload)
        {
            return getMessage(payload, 0x02);
        }

        public static byte[] getJsonMessage(byte[] payload)
        {
            return getMessage(payload, 0x01);
        }

        public static byte[] getJsonMessage(string message)
        {
            return getJsonMessage(Encoding.ASCII.GetBytes(message));
        }


    }
}
