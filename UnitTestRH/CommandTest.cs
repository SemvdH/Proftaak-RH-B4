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
            string messageId = "scene/terrain/add";

            Command command = new Command(testTunnelID);



            int[] terrainSizeArray = new int[2] { 4, 4 };
            float[] terrainHeightsArray = new float[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            string terrainAddCommand = command.TerrainAdd(terrainSizeArray, terrainHeightsArray, testSerial);

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);

            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test message
            Assert.AreEqual(messageId, (string)json.data.data.id);
            Assert.AreEqual(testSerial, (string)json.data.data.serial);

            //Test terrain
            JArray jArrayTerrainSize = (JArray)json.data.data.data.size;
            JArray jArrayTerrainHeights = (JArray)json.data.data.data.heights;

            int[] outSizeArray = jArrayTerrainSize.Select(ja => (int)ja).ToArray();
            float[] outHeightsArray = jArrayTerrainHeights.Select(ja => (float)ja).ToArray();

            CollectionAssert.AreEqual(terrainSizeArray, outSizeArray);
            CollectionAssert.AreEqual(terrainHeightsArray, json.data.data.data.heights.ToObject<float[]>());
        }

        [TestMethod]
        public void AddLayer_TestMethod()
        {
            string testTunnelID = "dummyTunnelID";
            string testSerial = "dummySerialCode";

            string payloadId = "tunnel/send";
            string messageId = "scene/node/addlayer";


            string testUuid = "dummyUuid";
            string diffuseExpected = @"data\NetworkEngine\textures\terrain\grass_green_d.jpg";
            string normalExpected = @"data\NetworkEngine\textures\terrain\grass_green_n.jpg";
            int minHeightExpected = 0;
            int maxHeightExpected = 10;
            int fadeDistExpected = 1;


            Command command = new Command(testTunnelID);
            
            string terrainAddCommand = command.AddLayer(testUuid, testSerial);

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);

            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test message
            Assert.AreEqual(messageId, (string)json.data.data.id);
            Assert.AreEqual(testSerial, (string)json.data.data.serial);

            //Test AddLayer
            Assert.AreEqual(testUuid, (string)json.data.data.data.id);
            Assert.AreEqual(diffuseExpected, (string)json.data.data.data.diffuse);
            Assert.AreEqual(normalExpected, (string)json.data.data.data.normal);
            Assert.AreEqual(minHeightExpected, (int)json.data.data.data.minHeight);
            Assert.AreEqual(maxHeightExpected, (int)json.data.data.data.maxHeight);
            Assert.AreEqual(fadeDistExpected, (int)json.data.data.data.fadeDist);
        }

        [TestMethod]
        public void UpdateTerrain_TestMethod()
        {
            string testTunnelID = "dummyTunnelID";

            string payloadId = "tunnel/send";
            string messageId = "scene/terrain/update";

            Command command = new Command(testTunnelID);

            string terrainAddCommand = command.UpdateTerrain();

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);

            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test message
            Assert.AreEqual(messageId, (string)json.data.data.id);
        }

        [TestMethod]
        public void renderTerrain_TestMethod()
        {
            string testTunnelID = "dummyTunnelID";
            string testSerial = "dummySerialCode";

            string payloadId = "tunnel/send";
            string messageId = "scene/node/add";


            string nameExpected = "newNode";
            int[] positionExpected = new int[] { -80, 0, -80 };
            float scaleExpected = 1f;
            int[] rotationExpected = new int[] { 0, 0, 0 };

            bool smoothnormalsExpected = true;


            Command command = new Command(testTunnelID);

            string terrainAddCommand = command.renderTerrain(testSerial);

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);


            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test message
            Assert.AreEqual(messageId, (string)json.data.data.id);
            Assert.AreEqual(testSerial, (string)json.data.data.serial);

            //Test data
            Assert.AreEqual(nameExpected, (string)json.data.data.data.name);

            //Test data components

                //Test transform
            JArray jArrayPosition = (JArray)json.data.data.data.components.transform.position;
            JArray jArrayRotation = (JArray)json.data.data.data.components.transform.rotation;

            int[] outPositionArray = jArrayPosition.Select(ja => (int)ja).ToArray();
            int[] outRotationArray = jArrayRotation.Select(ja => (int)ja).ToArray();

            CollectionAssert.AreEqual(positionExpected, outPositionArray);
            CollectionAssert.AreEqual(rotationExpected, outRotationArray);


            //Test terrain
            Assert.AreEqual(smoothnormalsExpected, (bool)json.data.data.data.components.terrain.smoothnormals);

        }

        [TestMethod]
        public void Dummy()
        {
            string testTunnelID = "dummyTunnelID";
            string testSerial = "dummySerialCode";

            string payloadId = "tunnel/send";
            string messageId = "scene/node/addlayer";

            Command command = new Command(testTunnelID);

            string terrainAddCommand = command.AddLayer("Dum", testSerial);

            dynamic json = JsonConvert.DeserializeObject(terrainAddCommand);

            //Test payload
            Assert.AreEqual(payloadId, (string)json.id);
            Assert.AreEqual(testTunnelID, (string)json.data.dest);

            //Test terrain
            Assert.AreEqual(messageId, (string)json.data.data.id);
            Assert.AreEqual(testSerial, (string)json.data.data.serial);
        }
    }
}
