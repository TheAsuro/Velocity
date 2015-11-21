using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Api
{
    public static class Api
    {
        public static void StartRequest(string url, string method, Action<ApiResult> callback, Dictionary<string, string> data = null)
        {
            try
            {
                string combinedUrl = url;

                if (data != null && method == "GET")
                {
                    bool first = true;

                    foreach (KeyValuePair<string, string> pair in data)
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

                HttpWebRequest request = WebRequest.Create(combinedUrl) as HttpWebRequest;
                request.Method = method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 2000;

                if (data != null && method != "GET")
                {
                    SendRequestData(request.GetRequestStream(), data);
                }

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    
                    callback(new ApiResult() { error = false, text = result });
                }
                else
                {
                    callback(new ApiResult() { error = true, text = result });
                }
            }
            catch (WebException ex)
            {
                UnityEngine.Debug.Log(ex.Response);
                callback(new ApiResult() { error = true, text = ex.Message });
            }
        }

        private static void SendRequestData(Stream stream, Dictionary<string, string> data)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            bool first = true;

            foreach (KeyValuePair<string, string> pair in data)
            {
                if (!first)
                    stream.Write(enc.GetBytes("&"), 0, enc.GetByteCount("&"));
                else
                    first = false;

                string content = pair.Key + "=" + pair.Value;
                stream.Write(enc.GetBytes(content), 0, enc.GetByteCount(content));
            }
        }

        public struct ApiResult
        {
            public bool error;
            public string text;
        }

        private struct RequestInfo
        {
            public RequestInfo(HttpWebRequest request, Dictionary<string, string> data)
            {
                this.request = request;
                this.data = data;
            }

            public HttpWebRequest request;
            public Dictionary<string, string> data;
        }
    }
}