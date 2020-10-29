using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Server
{
    class SaveData
    {
        private string path;
        private const string jsonFilename = "/json.txt";
        private const string rawBikeFilename = "/rawBike.bin";
        private const string rawBPMFilename = "/rawBPM.bin";
        public SaveData(string path)
        {
            this.path = path;
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
            using (StreamWriter sw = File.AppendText(this.path + jsonFilename))
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
            WriteRawData(data, this.path + rawBPMFilename);
        }

        public void WriteDataRAWBike(byte[] data)
        {
            if (data.Length != 8)
            {
                throw new ArgumentException("data should have length of 8");
            }
            WriteRawData(data, this.path + rawBikeFilename);
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
            using (var fileStream = new FileStream(fileLocation, FileMode.Append, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fileStream))
            {
                //sw.BaseStream.Seek(length, SeekOrigin.Begin);
                bw.Write(data);
                bw.Flush();
                //Console.WriteLine("wrote at " + bw.BaseStream.Position);
            }
            //using (BinaryReader binaryReader = new BinaryReader(File.Open(fileLocation, FileMode.Open)))
            //{
            //    byte[] totalArray = new byte[binaryReader.BaseStream.Length];
            //    binaryReader.BaseStream.Read(totalArray, 0, (int)binaryReader.BaseStream.Length);
            //    Console.WriteLine("all data is " + BitConverter.ToString(totalArray));
            //}
        }

        /// <summary>
        /// gets BPM graph data out of file.
        /// if you want 100 datapoints but here are onlny 50, de last 50 datapoint will be 0
        /// if you want 100 datapoints where it takes the average of 2, the last 75 will be 0
        /// if the file isn't created yet it will retun null
        /// </summary>
        /// <param name="outputSize">the amount of data points for the output</param>
        /// <param name="averageOver">the amount of data points form the file for one data point in the output</param>
        /// <returns>byte array with data points from file</returns>
        public byte[] getBPMgraphData(int outputSize, int averageOver)
        {
            if (File.Exists(this.path + rawBPMFilename))
            {
                FileInfo fi = new FileInfo(this.path + rawBPMFilename);
                int length = (int)fi.Length;
                //Console.WriteLine("length " + length);

                byte[] output = new byte[outputSize];

                int messageSize = 2;
                int readSize = messageSize * averageOver;
                byte[] readBuffer = new byte[readSize];

                using (BinaryReader binaryReader = new BinaryReader(File.Open(this.path + rawBPMFilename, FileMode.Open)))
                {
                    //byte[] totalArray = new byte[binaryReader.BaseStream.Length];
                    //binaryReader.BaseStream.Read(totalArray, 0, (int)binaryReader.BaseStream.Length);
                    //Console.WriteLine("all data is " + BitConverter.ToString(totalArray));
                    for (int i = 1; i <= outputSize; i++)
                    {
                        if (length - (i * readSize) < 0)
                        {
                            break;
                        }
                        binaryReader.BaseStream.Seek(length - (i * readSize), SeekOrigin.Begin);
                        binaryReader.BaseStream.Read(readBuffer, 0, readSize);
                        //Console.WriteLine("read " + binaryReader.BaseStream.Position + " and size " + readSize + " with value " + BitConverter.ToString(readBuffer));

                        //handling data
                        int total = 0;
                        for (int j = 0; j < averageOver; j++)
                        {
                            total += readBuffer[j * messageSize + 1];
                        }
                        output[i - 1] = (byte)(total / averageOver);
                    }
                }

                return output;
            }
            else
            {
                return null;
            }
        }


    }
}
