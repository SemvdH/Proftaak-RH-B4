using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    class SaveData
    {
        private string path;
        public SaveData(string path)
        {
            this.path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void WriteDataJSON(string data)
        {
            using (StreamWriter sw = File.AppendText(this.path + "/dataJSON.txt"))
            {
                sw.WriteLine(data);
            }
        }

        public void WriteDataRAW(string data)
        {
            using (StreamWriter sw = File.AppendText(this.path + "/dataRAW.txt"))
            {
                sw.WriteLine(data);
            }
        }
    }
}
