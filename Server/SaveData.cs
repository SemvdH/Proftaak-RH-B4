using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    class SaveData
    {
        public SaveData()
        {
        }

        public void WriteData(string data)
        {
            using (StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + "/data.txt"))
            {
                sw.WriteLine(data);
            }
        }
    }
}
