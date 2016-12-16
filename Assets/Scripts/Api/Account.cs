using UnityEngine;
using System.Collections.Generic;
using System;

namespace Api
{
    public class Account
    {
        private const string LOGIN_API_URL = "https://theasuro.de/Velocity/Api/account.php";

        public event EventHandler<EventArgs<string>> OnLoginFinished;
        public event EventHandler<EventArgs<string>> OnAccountRequestFinished;

        public string Name { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string Token { get; private set; }

        public Account(string name)
        {
            Name = name;
            IsLoggedIn = false;
            if (PlayerPrefs.HasKey(SaveData.SaveName(name) + "_token"))
            {
                Token = PlayerPrefs.GetString(SaveData.SaveName(name) + "_token");
                StartLoginCheck((result) => IsLoggedIn = result);
            }
        }

        public void StartCreate(string pass, string mail = "")
        {
            var data = new Dictionary<string, string>();
            data.Add("user", Name);
            data.Add("pass", pass);
            if (mail != "")
                data.Add("mail", mail);
            HttpApi.StartRequest(LOGIN_API_URL, "POST", FinishCreate, data);
        }

        private void FinishCreate(HttpApi.ApiResult result)
        {
            if (!result.error)
            {
                FinishLogin(result);
            }

            if (OnAccountRequestFinished != null)
                OnAccountRequestFinished(this, new EventArgs<string>(result.text, result.error, result.errorText));
        }

        public void StartLogin(string pass)
        {
            var data = new Dictionary<string, string>();
            data.Add("user", Name);
            data.Add("pass", pass);
            HttpApi.StartRequest(LOGIN_API_URL, "GET", FinishLogin, data);
        }

        private void FinishLogin(HttpApi.ApiResult result)
        {
            if (!result.error)
            {
                Token = result.text;

                PlayerPrefs.SetString(SaveData.SaveName(Name) + "_token", Token);
                IsLoggedIn = true;
                Debug.Log("Logged in.");
            }
            else
            {
                Debug.Log(result.errorText);
            }

            if (OnLoginFinished != null)
                OnLoginFinished(this, new EventArgs<string>(Token, result.error, result.errorText));
        }

        public void StartLoginCheck(Action<bool> callback)
        {
            var data = new Dictionary<string, string>();
            data.Add("token", Token);
            HttpApi.StartRequest(LOGIN_API_URL, "POST", (result) => { if (result.text == "1") callback(true); else callback(false); }, data);
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Content { get; private set; }
        public bool Error { get; private set; }
        public string ErrorText { get; private set; }

        public EventArgs(T content) : base()
        {
            Content = content;
            Error = false;
        }

        public EventArgs(T content, bool error, string errorText) : base()
        {
            Content = content;
            Error = error;
            ErrorText = errorText;
        }
    }
}