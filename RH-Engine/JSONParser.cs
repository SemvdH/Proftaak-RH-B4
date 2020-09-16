using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace RH_Engine
{
    class JSONParser
    {
        /// <summary>
        /// parses the given response from the server into strings
        /// </summary>
        /// <param name="msg">the message gotten from the server, without the length prefix</param>
        /// <returns></returns>
        public static string[] Parse(string msg)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(msg);
            Newtonsoft.Json.Linq.JArray data = jsonData.data;
            foreach (dynamic d in data)
            {
                Console.WriteLine(d.clientinfo.host);
            }

            return null;

        }

        
    }
}
