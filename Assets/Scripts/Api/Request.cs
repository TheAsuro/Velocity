using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Api
{
    public interface Request
    {
        bool Done { get; }
        bool Error { get; }
        string ErrorText { get; }
    }

    public class ApiRequest : Request
    {
        public event EventHandler<RequestFinishedEventArgs<string>> OnDone;

        public bool Done { get; private set; }
        public bool Error { get; private set; }
        public string ErrorText { get; private set; }
        public string Result { get; private set; }
        public HttpWebRequest HttpWebRequest { get; private set; }
        public Dictionary<string, string> RequestData { get; private set; }

        public ApiRequest(string url, string method, Dictionary<string, string> data = null)
        {
            RequestData = data;

            try
            {
                string combinedUrl = url;

                if (RequestData != null && method == "GET")
                {
                    bool first = true;

                    foreach (KeyValuePair<string, string> pair in RequestData)
                    {
                        if (!first)
                            combinedUrl += "&";
                        else
                        {
                            combinedUrl += "?";
                            first = false;
                        }

                        combinedUrl += pair.Key + "=" + pair.Value;
                    }
                }

                HttpWebRequest = (HttpWebRequest) WebRequest.Create(combinedUrl);
                HttpWebRequest.Method = method;
                HttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                HttpWebRequest.Timeout = 3000;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
                {
                    bool isOk = true;
                    // If there are errors in the certificate chain, look at each error to determine the cause.
                    if (errors != SslPolicyErrors.None)
                    {
                        foreach (X509ChainStatus status in chain.ChainStatus)
                        {
                            if (status.Status != X509ChainStatusFlags.RevocationStatusUnknown)
                            {
                                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                                bool chainIsValid = chain.Build((X509Certificate2) certificate);
                                if (!chainIsValid)
                                {
                                    isOk = false;
                                }
                            }
                        }
                    }
                    return isOk;
                };
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        public void StartRequest()
        {
            if (RequestData != null && HttpWebRequest.Method != "GET")
            {
                HttpWebRequest.BeginGetRequestStream(ContinueRequest, null);
            }
            else
            {
                HttpWebRequest.BeginGetResponse(ProcessResponse, null);
            }
        }

        private void ContinueRequest(IAsyncResult result)
        {
            try
            {
                using (Stream stream = HttpWebRequest.EndGetRequestStream(result))
                {
                    using (StreamWriter sw = new StreamWriter(stream, Encoding.ASCII))
                    {
                        bool first = true;

                        foreach (KeyValuePair<string, string> pair in RequestData)
                        {
                            if (!first)
                                sw.Write("&");
                            else
                                first = false;

                            sw.Write(pair.Key + "=" + pair.Value);
                        }

                        sw.Flush();
                    }
                }

                HttpWebRequest.BeginGetResponse(ProcessResponse, null);
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        private void ProcessResponse(IAsyncResult result)
        {
            try
            {
                WebResponse response = HttpWebRequest.EndGetResponse(result);
                Result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Finish();
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        private void HandleWebException(WebException ex)
        {
            Error = true;
            if (ex.Response != null)
            {
                using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    ErrorText = ex.Message + "\n" + reader.ReadToEnd();
                }
            }
            else
            {
                ErrorText = ex.Message;
            }
            Finish();
        }

        private void Finish()
        {
            Done = true;
            if (OnDone != null)
                OnDone(this, new RequestFinishedEventArgs<string>(Error, ErrorText, Result));
        }
    }

    public class RequestFinishedEventArgs<T> : EventArgs
    {
        public bool Error { get; private set; }
        public string ErrorText { get; private set; }
        public T Result { get; private set; }

        public RequestFinishedEventArgs(bool error, string errorText, T result)
        {
            Error = error;
            ErrorText = errorText;
            Result = result;
        }
    }
}