using System.Globalization;
using System.IO;
using Api;
using Demos;
using Game;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Util;

namespace Tests
{
    public class LeaderboardTest
    {
        [Test]
        public void SendPreparationsTest()
        {
            PlayerSave save = Substitute.ForPartsOf<PlayerSave>("testplayer");
            ApiRequest request = Leaderboard.SendEntry(save, -1, 123400000, null);

            StringRequestData data = (StringRequestData) request.RequestData;
            Assert.AreEqual("-1", data["User"]);
            Assert.AreEqual(12.34m, decimal.Parse(data["Time"], CultureInfo.InvariantCulture));
            Assert.AreEqual("-1", data["Map"]);
            Assert.AreEqual(null, data["Token"]);
        }

        [Test]
        public void QueryTest()
        {
            UnityUtils.RunWhenDone(Leaderboard.GetRecord(new MapData() {id = 9}), (request) =>
            {
                Assert.IsFalse(request.Error);
                if (request.Error)
                    Debug.LogError(request.Error);

                Debug.Log(request.Result[0].Time);
            }).RunSynchronously();
        }

        [Test]
        public void DemoTest()
        {
            byte[] streamData;
            Demo testDemo = new Demo("12d22e1a-9851-421a-a8d0-9ca85487e499");

            using (MemoryStream stream = new MemoryStream())
            {
                testDemo.SaveToStream(stream);
                streamData = stream.ToArray();
            }

            // test if demo was correctly written to stream
            Demo testDemo2 = new Demo(streamData);
            AssertDemosEqual(testDemo, testDemo2);

            // send demo to server
            RequestData demoData = new BinaryRequestData(streamData);
            ApiRequest sendRq = new ApiRequest(Url.DEMOS + "/34/1", "POST", demoData);
            sendRq.OnDone += (o, eventArgs) => { Debug.Log(sendRq.Error ? sendRq.ErrorText : "Demo upload finished!"); };
            sendRq.StartRequest();
            while(!sendRq.Done) {}

            Assert.IsFalse(sendRq.Error);
            Debug.Log(sendRq.Error ? sendRq.ErrorText : "Send succeded.");

            // request demo from server
            ApiRequest recvRq = new ApiRequest(Url.DEMOS + "/34/1", "GET");
            recvRq.OnDone += (sender, args) =>
            {
                if (args.Error)
                    Debug.Log(args.ErrorText);
                else
                {
                    Demo demo = new Demo(args.BinaryResult);
                    Debug.Log(demo.PlayerName);
                }
            };
            recvRq.StartRequest();
            while (!recvRq.Done) {}

            Assert.IsFalse(recvRq.Error);
            Debug.Log(sendRq.Error ? sendRq.ErrorText : "Recieve succeded.");

            // check if server returned what we sent
            for (int i = 0; i < streamData.Length; i++)
            {
                Assert.AreEqual(streamData[i], recvRq.BinaryResult[i]);
                if (streamData[i] != recvRq.BinaryResult[i])
                    Debug.Log("Bytes differ at index " + i);
            }

            Demo result = new Demo(recvRq.BinaryResult);
            AssertDemosEqual(testDemo, result);
        }

        private static void AssertDemosEqual(Demo demo1, Demo demo2)
        {
            for (int i = 0; i < demo1.Ticks.Count; i++)
            {
                Assert.AreEqual(demo1.Ticks[i].Time, demo2.Ticks[i].Time);
            }
        }
    }
}