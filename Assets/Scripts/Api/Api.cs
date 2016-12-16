using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;

namespace Api
{
    public static class HttpApi
    {
        private static Dictionary<Action<ApiResult>, ApiResult> callbacks = new Dictionary<Action<ApiResult>, ApiResult>();

        public static void ConsumeCallbacks()
        {
            var tempDict = new Dictionary<Action<ApiResult>, ApiResult>(callbacks);
            callbacks.Clear();

            foreach (KeyValuePair<Action<ApiResult>, ApiResult> callback in tempDict)
            {
                callback.Key(callback.Value);
            }
        }

        public static void StartRequest(string url, string method, Action<ApiResult> callback = null, Dictionary<string, string> data = null)
        {
#if UNITY_STANDALONE
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
                request.Timeout = 3000;

                if (data != null && method != "GET")
                {
                    request.BeginGetRequestStream(new AsyncCallback(ContinueRequest), new RequestInfo() { request = request, data = data, callback = callback });
                }
                else
                {
                    request.BeginGetResponse(new AsyncCallback(ProcessResponse), new ResponseInfo() { request = request, callback = callback });
                }
            }
            catch (WebException ex)
            {
                HandleWebException(ex, callback);
            }
#else
            if (callback != null)
                callbacks.Add(callback, new ApiResult() { error = true, text = "", errorText = "Web requests are not possible in the web player." });
#endif
        }

        private static void ContinueRequest(IAsyncResult result)
        {
            RequestInfo info = (RequestInfo)result.AsyncState;

            try
            {
                using (Stream stream = info.request.EndGetRequestStream(result))
                {
                    using (StreamWriter sw = new StreamWriter(stream, Encoding.ASCII))
                    {
                        bool first = true;

                        foreach (KeyValuePair<string, string> pair in info.data)
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

                info.request.BeginGetResponse(new AsyncCallback(ProcessResponse), new ResponseInfo() { request = info.request, callback = info.callback });
            }
            catch (WebException ex)
            {
                HandleWebException(ex, info.callback);
            }
        }

        private static void ProcessResponse(IAsyncResult result)
        {
            ResponseInfo info = (ResponseInfo)result.AsyncState;

            try
            {
                WebResponse response = info.request.EndGetResponse(result);

                string resultStr = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (info.callback != null)
                    callbacks.Add(info.callback, new ApiResult() { error = false, text = resultStr, errorText = "" });
            }
            catch (WebException ex)
            {
                HandleWebException(ex, info.callback);
            }
        }

        private static void HandleWebException(WebException ex, Action<ApiResult> callback)
        {
            if (ex.Response != null)
            {
                StreamReader reader = new StreamReader(ex.Response.GetResponseStream());
                if (callback != null)
                    callbacks.Add(callback, new ApiResult() { error = true, text = "", errorText = ex.Message + "\n" + reader.ReadToEnd() });
            }
            else
            {
                if (callback != null)
                    callbacks.Add(callback, new ApiResult() { error = true, text = "", errorText = ex.Message });
            }
        }

        public struct ApiResult
        {
            public bool error;
            public string text;
            public string errorText;
        }

        private struct RequestInfo
        {
            public HttpWebRequest request;
            public Dictionary<string, string> data;
            public Action<ApiResult> callback;
        }

        private struct ResponseInfo
        {
            public HttpWebRequest request;
            public Action<ApiResult> callback;
        }
    }
}