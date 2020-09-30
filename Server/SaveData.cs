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
            using (StreamWriter sw = File.AppendText(this.path + "/json" + filename + ".txt"))
            {
                sw.WriteLine(data);
            }
        }

        public void WriteDataRAWBPM(byte[] data)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("data should have length of 2");
            }
            WriteRawData(data, this.path + "/rawBPM" + filename + ".bin");
        }

        public void WriteDataRAWBike(byte[] data)
        {
            if (data.Length != 8)
            {
                throw new ArgumentException("data should have length of 8");
            }
            WriteRawData(data, this.path + "/rawBike" + filename + ".bin");
        }

        private void WriteRawData(byte[] data, string fileLocation)
        {
            int length = 0;
            try
            {
                FileInfo fi = new FileInfo(fileLocation);
                length = (int)fi.Length;
            }
            catch
            {
                // do nothing
            }
            using (BinaryWriter sw = new BinaryWriter(File.Open(fileLocation, FileMode.Create)))
            {
                sw.Seek(length, SeekOrigin.End);
                sw.Write(data);
                sw.Flush();
            }
        }


    }
}
