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
        /// returns all the users from the given response
        /// </summary>
        /// <param name="msg">the message gotten from the server, without the length prefix</param>
        /// <returns></returns>
        public static PC[] GetUsers(string msg)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(msg);
            Newtonsoft.Json.Linq.JArray data = jsonData.data;
            PC[] res = new PC[data.Count];
            int counter = 0;
            foreach (dynamic d in data)
            {
                res[counter] = new PC((string)d.clientinfo.host, (string)d.clientinfo.user);
                counter++;
            }

            return res;

        }

        public static string GetSessionID(string msg, PC[] PCs)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(msg);
            Newtonsoft.Json.Linq.JArray data = jsonData.data;
            for (int i = data.Count-1; i >= 0; i--)
            {
                dynamic d = data[i];
                foreach (PC pc in PCs)
                {
                    if (d.clientinfo.host == pc.host && d.clientinfo.user == pc.user)
                    {
                        Console.WriteLine("connecting to {0}, on {1} with id {2}", pc.user, pc.host, d.id);
                        return d.id;
                    }
                }
            }

            return null;
        }

        public static string GetTunnelID(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);
            if (jsonData.data.status == "ok")
            {
                return jsonData.data.id;
            }
            return null;
        }

        public static string GetRouteID(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);
            if (jsonData.data.status == "ok")
            {
                return jsonData.data.uuid;
            }
            return null;
        }

        public static string getPanelID(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);
            if (jsonData.data.data.data.name == "dashboard")
            {
                Console.WriteLine(jsonData.data.data.data.uuid);
                return jsonData.data.data.data.uuid;
            }
            return null;
        }
    }
}
