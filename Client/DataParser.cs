using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    }
}
