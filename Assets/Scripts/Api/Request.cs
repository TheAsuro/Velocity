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
        public event EventHandler<RequestFinishedEventArgs> OnDone;

        public bool Done { get; private set; }
        public bool Error { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorText { get; private set; }
        public byte[] BinaryResult { get; private set; }
        public HttpWebRequest HttpWebRequest { get; private set; }
        public RequestData RequestData { get; private set; }

        public string StringResult
        {
            get { return Encoding.UTF8.GetString(BinaryResult); }
        }

        public ApiRequest(string url, string method, RequestData data = null)
        {
            RequestData = data;
            string combinedUrl = url;

            if (RequestData != null && method == "GET")
            {
                combinedUrl += RequestData.ToString();
            }
            CreateRequest(method, combinedUrl);
        }

        private void CreateRequest(string method, string url)
        {
            try
            {
                HttpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                HttpWebRequest.Method = method;
                if (RequestData != null)
                    HttpWebRequest.ContentType = RequestData.GetContentType();
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
                ErrorCode = -1;
                HandleWebException(ex);
            }
        }

        public void StartRequest()
        {
            if (RequestData != null && HttpWebRequest.Method != "GET")
            {
                HttpWebRequest.BeginGetRequestStream(SendRequestBody, null);
            }
            else
            {
                HttpWebRequest.BeginGetResponse(ProcessResponse, null);
            }
        }

        private void SendRequestBody(IAsyncResult result)
        {
            try
            {
                using (Stream stream = HttpWebRequest.EndGetRequestStream(result))
                {
                    RequestData.WriteData(stream);
                }

                HttpWebRequest.BeginGetResponse(ProcessResponse, null);
            }
            catch (WebException ex)
            {
                ErrorCode = -2;
                HandleWebException(ex);
            }
        }

        private void ProcessResponse(IAsyncResult result)
        {
            try
            {
                WebResponse response = HttpWebRequest.EndGetResponse(result);
                Stream responseStream = response.GetResponseStream();
                List<byte> downloadBuffer = new List<byte>(); // TODO: @optimize probably slow
                int byteResult;

                while ((byteResult = responseStream.ReadByte()) >= 0)
                {
                    downloadBuffer.Add((byte)byteResult);
                }

                BinaryResult = downloadBuffer.ToArray();
                Finish();
            }
            catch (WebException ex)
            {
                ErrorCode = -3;
                HandleWebException(ex);
            }
        }

        private void Finish()
        {
            Done = true;
            if (OnDone != null)
                OnDone(this, new RequestFinishedEventArgs(Error, ErrorCode, ErrorText, BinaryResult));
        }

        private void HandleWebException(WebException ex)
        {
            Error = true;

            if (ex.Response != null)
            {
                ErrorCode = (int)((HttpWebResponse) ex.Response).StatusCode;
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
    }

    public class RequestFinishedEventArgs : EventArgs
    {
        public bool Error { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorText { get; private set; }
        public byte[] BinaryResult { get; private set; }

        public string StringResult
        {
            get { return Encoding.UTF8.GetString(BinaryResult); }
        }

        public RequestFinishedEventArgs(bool error, int errorCode, string errorText, byte[] result)
        {
            Error = error;
            ErrorCode = errorCode;
            ErrorText = errorText;
            BinaryResult = result;
        }
    }
}