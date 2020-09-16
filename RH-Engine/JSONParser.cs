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
                res[counter] = new PC(d.clientinfo.host, d.clientinfo.user);
                counter++;
            }

            return res;

        }

        public static string GetSessionID(string msg, PC[] PCs)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(msg);
            Newtonsoft.Json.Linq.JArray data = jsonData.data;
            foreach (dynamic d in data)
            {
                foreach(PC pc in PCs)
                {
                    if (d.clientinfo.host == pc.host && d.clientinfo.user == pc.user)
                    {
                        return d.id;
                    }
                }
            }

            return null;
        }


    }
}
