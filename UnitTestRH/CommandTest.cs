using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProftaakRH;
using RH_Engine;
using System;
using System.Linq;

namespace UnitTestRH
{
    [TestClass]
    public class CommandTest
    {
        [TestMethod]
        public void TerrainAdd_TestMethod()
        {
            string testTunnelID = "dummyTunnelID";
            string testSerial = "dummySerialCode";

            string payloadId = "tunnel/send";
            string terrainId = "scene/terrain/add";

            Command command = new Command(testTunnelID);



            int[] terrainSizeArray = new int[2] { 4, 4 };
            float[] terrainHeightsArray = new float[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            string terrainAddCommand = command.TerrainAdd(terrainSizeArray, terrainHeightsArray, testSerial);

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);

            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test terrain
            Assert.AreEqual(terrainId, (string)json.data.data.id);
            Assert.AreEqual(testSerial, (string)json.data.data.serial);

            //Test terrain settings
            JArray jArrayTerrainSize = (JArray)json.data.data.data.size;
            Assert.AreEqual(terrainSizeArray, jArrayTerrainSize.ToObject<float[]>());
            //Assert.AreEqual(terrainHeightsArray, (float[])json.data.data.data.heights);
        }
    }
}
