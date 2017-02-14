using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Util;

namespace Game
{
    public class PlayerSave
    {
        private const string LOGIN_API_URL = "https://api.theasuro.de/velocity/login";
        private static readonly string PLAYER_SAVE_DIR = Path.Combine("data", "players");

        public static PlayerSave current;

        public event EventHandler<EventArgs<string>> OnLoginFinished;
        public event EventHandler<EventArgs<string>> OnAccountRequestFinished;
        public event EventHandler<EventArgs<bool>> OnLoginCheckFinished;

        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string Token { get; private set; }

        private Dictionary<int, long[]> personalBestTimes = new Dictionary<int, long[]>();

        public static PlayerSave LoadFromFile(string playerName)
        {
            return JsonConvert.DeserializeObject<PlayerSave>(File.ReadAllText(GetFilePath(playerName)), new JsonSerializerSettings()
            {
                ContractResolver = new PrivateContractResolver()
            });
        }

        public PlayerSave(string name)
        {
            Name = name;
            IsLoggedIn = false;
        }

        public bool SaveTimeIfPersonalBest(long[] time, MapData map)
        {
            long[] oldTime;
            if (GetPersonalBest(map, out oldTime))
            {
                if (time.Last() < oldTime.Last())
                {
                    personalBestTimes[map.id] = time;
                    SaveFile();
                    return true;
                }
                return false;
            }
            personalBestTimes[map.id] = time;
            SaveFile();
            return true;
        }

        public bool GetPersonalBest(MapData map, out long[] time)
        {
            if (personalBestTimes.ContainsKey(map.id))
            {
                time = personalBestTimes[map.id];
                return true;
            }
            time = new long[0];
            return false;
        }

        public void StartCreate(string pass, string mail = "")
        {
            var data = new Dictionary<string, string> {{"Name", Name}, {"Key", pass}};
            if (mail != "")
                data.Add("Mail", mail);
            ApiRequest rq = new ApiRequest(LOGIN_API_URL, "PUT", data);
            rq.OnDone += FinishCreate;
            rq.StartRequest();
        }

        private void FinishCreate(object o, RequestFinishedEventArgs<string> eventArgs)
        {
            if (!eventArgs.Error)
            {
                AccountCreationResult account = JsonConvert.DeserializeObject<AccountCreationResult>(eventArgs.Result);
                ID = account.ID;
                DoLogin(account.Token);
            }

            if (OnAccountRequestFinished != null)
                OnAccountRequestFinished(this, new EventArgs<string>(eventArgs.Result, eventArgs.Error, eventArgs.ErrorText));
        }

        public void StartLogin(string pass)
        {
            var data = new Dictionary<string, string> {{"User", Name}, {"pass", pass}};
            ApiRequest rq = new ApiRequest(LOGIN_API_URL, "POST", data);
            rq.OnDone += FinishLogin;
            rq.StartRequest();
        }

        private void FinishLogin(object sender, RequestFinishedEventArgs<string> eventArgs)
        {
            if (!eventArgs.Error)
                DoLogin(eventArgs.Result);

            if (OnLoginFinished != null)
                OnLoginFinished(this, new EventArgs<string>(Token, eventArgs.Error, eventArgs.ErrorText));
        }

        private void DoLogin(string token)
        {
            Token = token;
            IsLoggedIn = true;
            SaveFile();
            Debug.Log("Logged in.");
        }

        public void StartLoginCheck()
        {
            var data = new Dictionary<string, string> {{"token", Token}};
            ApiRequest rq = new ApiRequest(LOGIN_API_URL, "POST", data);
            rq.OnDone += (sender, eventArgs) =>
            {
                IsLoggedIn = !eventArgs.Error && eventArgs.Result == "1";
                if (OnLoginCheckFinished != null)
                    OnLoginCheckFinished(this, new EventArgs<bool>(IsLoggedIn, eventArgs.Error, eventArgs.ErrorText));
            };
            rq.StartRequest();
        }

        public void SaveFile()
        {
            File.WriteAllText(GetFilePath(Name), JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                ContractResolver = new PrivateContractResolver()
            }));
        }

        public void DeleteFile()
        {
            if (File.Exists(GetFilePath(Name)))
                File.Delete(GetFilePath(Name));
        }

        public static bool PlayerFileExists(string playerName)
        {
            return File.Exists(GetFilePath(playerName));
        }

        private static string GetFilePath(string playerName)
        {
            return Path.Combine(PLAYER_SAVE_DIR, playerName + ".vsav");
        }

        private class AccountCreationResult
        {
            public int ID { get; private set; }
            public string Token { get; private set; }
        }

        private class PrivateContractResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                List<MemberInfo> baseMembers = base.GetSerializableMembers(objectType);
                baseMembers.Add(objectType.GetField("personalBestTimes", BindingFlags.Instance | BindingFlags.NonPublic));
                return baseMembers;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, MemberSerialization.Fields);
            }
        }
    }
}