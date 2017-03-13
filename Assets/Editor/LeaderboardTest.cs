using System.Collections;
using System.Globalization;
using Api;
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
        public void SendTest()
        {
            PlayerSave save = Substitute.ForPartsOf<PlayerSave>("testplayer");
            ApiRequest request = Leaderboard.SendEntry(save, -1, 123400000, null);

            Assert.AreEqual("-1", request.RequestData["User"]);
            Assert.AreEqual(12.34m, decimal.Parse(request.RequestData["Time"], CultureInfo.InvariantCulture));
            Assert.AreEqual("-1", request.RequestData["Map"]);
            Assert.AreEqual(null, request.RequestData["Token"]);
        }

        [Test]
        public void QueryTest()
        {
            IEnumerator enumerator = UnityUtils.RunWhenDone(Leaderboard.GetRecord(new MapData() {id = 9}), (request) =>
            {
                Assert.IsFalse(request.Error);
                if (request.Error)
                    Debug.LogError(request.Error);

                Debug.Log(request.Result[0].time);
            });
            enumerator.RunSynchronously();
        }
    }
}