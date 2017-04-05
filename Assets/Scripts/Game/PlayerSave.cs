using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Api;
using Newtonsoft.Json;
using UI;
using UnityEngine.Assertions;
using Util;

namespace Game
{
    public class PlayerSave
    {
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
            SerializablePlayerSave save = JsonConvert.DeserializeObject<SerializablePlayerSave>(File.ReadAllText(GetFilePath(playerName)));
            return new PlayerSave(save);
        }

        public PlayerSave(string name)
        {
            Name = name;
            IsLoggedIn = false;
            ID = -1;
        }

        private PlayerSave(SerializablePlayerSave save)
        {
            personalBestTimes = save.PersonalBestTimes;
            ID = save.ID;
            Name = save.Name;
            Token = save.Token;
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
            StringRequestData data = new StringRequestData {{"Name", Name}, {"Key", pass}};
            if (mail != "")
                data.Add("Mail", mail);
            ApiRequest rq = new ApiRequest(Url.LOGIN, "PUT", data);
            rq.OnDone += FinishCreate;
            rq.StartRequest();
        }

        private void FinishCreate(object o, RequestFinishedEventArgs eventArgs)
        {
            if (!eventArgs.Error)
            {
                AccountCreationResult account = JsonConvert.DeserializeObject<AccountCreationResult>(eventArgs.StringResult);
                Assert.IsTrue(account.ID != 0);
                Assert.IsNotNull(account.Token);

                ID = account.ID;
                DoLogin(account.Token);
            }

            if (OnAccountRequestFinished != null)
                OnAccountRequestFinished(this, new EventArgs<string>(eventArgs.StringResult, eventArgs.Error, eventArgs.ErrorText));
        }

        public void StartLogin(string pass)
        {
            if (ID == -1)
            {
                StringRequestData data = new StringRequestData {{"Name", Name}};
                ApiRequest rq = new ApiRequest(Url.LOGIN, "GET", data);
                rq.OnDone += (sender, args) =>
                {
                    if (args.Error)
                    {
                        GameMenu.SingletonInstance.ShowError(args.ErrorText);
                        if (OnLoginFinished != null)
                        {
                            OnLoginFinished(this, new EventArgs<string>(args.StringResult, args.Error, args.ErrorText));
                            OnLoginFinished = null;
                        }
                    }
                    else
                    {
                        ID = int.Parse(args.StringResult);
                        ContinueLogin(pass);
                    }
                };
                rq.StartRequest();
            }
            else
            {
                ContinueLogin(pass);
            }
        }

        private void ContinueLogin(string pass)
        {
            StringRequestData data = new StringRequestData {{"User", ID.ToString()}, {"Key", pass}};
            ApiRequest rq = new ApiRequest(Url.LOGIN, "POST", data);
            rq.OnDone += FinishLogin;
            rq.StartRequest();
        }

        private void FinishLogin(object sender, RequestFinishedEventArgs eventArgs)
        {
            if (!eventArgs.Error)
            {
                string tokenString = eventArgs.StringResult;
                if (tokenString.StartsWith("\""))
                    tokenString = tokenString.Trim('"');
                DoLogin(tokenString);
            }

            if (OnLoginFinished != null)
            {
                OnLoginFinished(this, new EventArgs<string>(Token, eventArgs.Error, eventArgs.ErrorText));
                OnLoginFinished = null;
            }
        }

        private void DoLogin(string token)
        {
            Token = token;
            IsLoggedIn = true;
            SaveFile();
        }

        public void SaveFile()
        {
            if (!Directory.Exists(Paths.PlayerSaveDir))
                Directory.CreateDirectory(Paths.PlayerSaveDir);

            SerializablePlayerSave save = new SerializablePlayerSave(personalBestTimes, ID, Name, Token);
            File.WriteAllText(GetFilePath(Name), JsonConvert.SerializeObject(save));
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
            return Path.Combine(Paths.PlayerSaveDir, playerName + ".vsav");
        }

        private class AccountCreationResult
        {
            public int ID { get; private set; }
            public string Token { get; private set; }
        }

        private class SerializablePlayerSave
        {
            public Dictionary<int, long[]> PersonalBestTimes { get; private set; }
            public int ID { get; private set; }
            public string Name { get; private set; }
            public string Token { get; private set; }

            public SerializablePlayerSave(Dictionary<int, long[]> personalBestTimes, int id, string name, string token)
            {
                PersonalBestTimes = personalBestTimes;
                ID = id;
                Name = name;
                Token = token;
            }
        }
    }
}