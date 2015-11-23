using UnityEngine;
using System.Collections.Generic;
using System;

namespace Api
{
    public class Account
    {
        private const string LOGIN_API_URL = "http://theasuro.de/Velocity/Test/account.php";

        public event EventHandler<StringEventArgs> OnLoginFinished;
        public event EventHandler OnAccountRequestFinished;

        public string Name { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string Token { get; private set; }

        public Account(string name)
        {
            Name = name;
            IsLoggedIn = false;
        }

        public void StartCreate(string pass, string mail = "")
        {
            var data = new Dictionary<string, string>();
            data.Add("user", Name);
            data.Add("pass", pass);
            if (mail != "")
                data.Add("mail", mail);
            Api.StartRequest(LOGIN_API_URL, "POST", FinishCreate, data);
        }

        private void FinishCreate(Api.ApiResult result)
        {
            if (result.error == false)
            {
                FinishLogin(result);
            }
            else
            {
                Debug.Log(result.text + " (" + result.errorText + ")");
            }

            if (OnAccountRequestFinished != null)
                OnAccountRequestFinished(this, new EventArgs());
        }

        public void StartLogin(string pass)
        {
            var data = new Dictionary<string, string>();
            data.Add("user", Name);
            data.Add("pass", pass);
            Api.StartRequest(LOGIN_API_URL, "GET", FinishLogin, data);
        }

        private void FinishLogin(Api.ApiResult result)
        {
            if (result.error == false)
            {
                Token = result.text;
                IsLoggedIn = true;
            }
            else
            {
                Debug.Log(result.text);
            }

            if (OnLoginFinished != null)
                OnLoginFinished(this, new StringEventArgs(Token));
        }
    }

    public class StringEventArgs : EventArgs
    {
        public string Content { get; private set; }

        public StringEventArgs(string str) : base()
        {
            Content = str;
        }
    }
}