using UnityEngine;
using System.Collections.Generic;
using System;
using Game;

namespace Api
{
    public class Account
    {
        private const string LOGIN_API_URL = "https://theasuro.de/Velocity/Api/account.php";

        public event EventHandler<EventArgs<string>> OnLoginFinished;
        public event EventHandler<EventArgs<string>> OnAccountRequestFinished;
        public event EventHandler<EventArgs<bool>> OnLoginCheckFinished;

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
                StartLoginCheck();
            }
        }

        public void StartCreate(string pass, string mail = "")
        {
            var data = new Dictionary<string, string> {{"user", Name}, {"pass", pass}};
            if (mail != "")
                data.Add("mail", mail);
            ApiRequest rq = new ApiRequest(LOGIN_API_URL, "POST", data);
            rq.OnDone += FinishCreate;
            rq.StartRequest();
        }

        private void FinishCreate(object o, RequestFinishedEventArgs<string> eventArgs)
        {
            FinishLogin(eventArgs);

            if (OnAccountRequestFinished != null)
                OnAccountRequestFinished(this, new EventArgs<string>(eventArgs.Result, eventArgs.Error, eventArgs.ErrorText));
        }

        public void StartLogin(string pass)
        {
            var data = new Dictionary<string, string> {{"user", Name}, {"pass", pass}};
            ApiRequest rq = new ApiRequest(LOGIN_API_URL, "GET", data);
            rq.StartRequest();
        }

        private void FinishLogin(RequestFinishedEventArgs<string> eventArgs)
        {
            if (!eventArgs.Error)
            {
                Token = eventArgs.Result;

                PlayerPrefs.SetString(SaveData.SaveName(Name) + "_token", Token);
                IsLoggedIn = true;
                Debug.Log("Logged in.");

                if (OnLoginFinished != null)
                    OnLoginFinished(this, new EventArgs<string>(Token, eventArgs.Error, eventArgs.ErrorText));
            }
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