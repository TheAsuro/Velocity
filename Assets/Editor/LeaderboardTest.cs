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
        public void DemoUploadTest()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new Demo("default").SaveToStream(stream);
                RequestData demoData = new BinaryRequestData(stream.ToArray());
                // TODO: get run id from highscore request
                ApiRequest demoRq = new ApiRequest(Url.DEMOS + "/34/1", "POST", demoData);
                demoRq.OnDone += (o, eventArgs) => { Debug.Log(demoRq.Error ? demoRq.ErrorText : "Demo upload finished!"); };
                demoRq.StartRequest();
            }
        }

        [Test]
        public void DemoDownloadTest()
        {
            ApiRequest rq = new ApiRequest(Url.DEMOS + "/34/1", "GET");
            rq.OnDone += (sender, args) =>
            {
                if (args.Error)
                    Debug.Log(args.ErrorText);
                else
                {
                    Demo demo = new Demo(args.BinaryResult);
                    Debug.Log(demo.PlayerName);
                }
            };
            rq.StartRequest();
        }
    }
}