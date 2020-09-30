using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    class SaveData
    {
        private string path;
        private string filename;
        public SaveData(string path, string filename)
        {
            this.path = path;
            this.filename = filename;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        ///  Every line is a new data entry
        /// </summary>
       
        public void WriteDataJSON(string data)
        {
            using (StreamWriter sw = File.AppendText(this.path + "/json"+filename+".txt"))
            {
                sw.WriteLine(data);
            }
        }

        public void WriteDataRAW(string data)
        {
            using (StreamWriter sw = File.AppendText(this.path + "/rawFiets" + filename + ".txt"))
            {
                sw.WriteLine(data);
            }
        }
    }
}
