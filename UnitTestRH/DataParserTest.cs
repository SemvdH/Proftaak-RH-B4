using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RH_Engine;
using System;
using System.Collections.Generic;
using System.Text;
using Util;

namespace UnitTestRH
{
    [TestClass]
    public class DataParserTest
    {

        public byte[] GetPayload(byte[] message)
        {
            byte[] payload = new byte[message.Length - 5];
            Array.Copy(message, 5, payload, 0, message.Length - 5);
            return payload;
        }


        [TestMethod]
        public void TestGetMessageToSend()
        {
            byte[] toTest = DataParser.GetMessageToSend("test");

            dynamic res = JsonConvert.DeserializeObject(Encoding.ASCII.GetString((toTest)));
            Assert.AreEqual("MESSAGE", (string)res.identifier);
            Assert.AreEqual("test", (string)res.data.message);
        }

        [TestMethod]
        public void TestIsRawDataBikeServer()
        {
            byte[] testArr = { 0x34,0x00,0x00,0x00,0x02};
            byte[] testArr2 = { 0x34, 0x00, 0x00, 0x00, 0x02,0x49,0x65 };
            Assert.ThrowsException<ArgumentException>(() => DataParser.isRawDataBikeServer(testArr));
            Assert.IsTrue(DataParser.isRawDataBikeServer(testArr2));
        }
        [TestMethod]
        public void TestIsRawDataBikeDoctor()
        {
            byte[] testArr = { 0x34, 0x00, 0x00, 0x00, 0x04 };
            byte[] testArr2 = { 0x34, 0x00, 0x00, 0x00, 0x04, 0x49, 0x65 };
            Assert.ThrowsException<ArgumentException>(() => DataParser.isRawDataBikeDoctor(testArr));
            Assert.IsTrue(DataParser.isRawDataBikeDoctor(testArr2));
        }

        [TestMethod]
        public void TestIsRawDataBPMServer()
        {
            byte[] testArr = { 0x34, 0x00, 0x00, 0x00, 0x03};
            byte[] testArr2 = { 0x34, 0x00, 0x00, 0x00, 0x03, 0x49, 0x65 };
            Assert.ThrowsException<ArgumentException>(() => DataParser.isRawDataBPMServer(testArr));
            Assert.IsTrue(DataParser.isRawDataBPMServer(testArr2));
        }

        [TestMethod]
        public void TestIsRawDataBPMDoctor()
        {
            byte[] testArr = { 0x34, 0x00, 0x00, 0x00, 0x05 };
            byte[] testArr2 = { 0x34, 0x00, 0x00, 0x00, 0x05, 0x49, 0x65 };
            Assert.ThrowsException<ArgumentException>(() => DataParser.isRawDataBPMDoctor(testArr));
            Assert.IsTrue(DataParser.isRawDataBPMDoctor(testArr2));
        }






    }
}
