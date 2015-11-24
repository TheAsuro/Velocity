using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Api
{
    public static class HttpApi
    {
        public static void StartRequest(string url, string method, Action<ApiResult> callback = null, Dictionary<string, string> data = null)
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
        }

        private static void ContinueRequest(IAsyncResult result)
        {
            RequestInfo info = (RequestInfo)result.AsyncState;

            try
            {
                Stream stream = info.request.EndGetRequestStream(result);
                StreamWriter sw = new StreamWriter(stream);

                ASCIIEncoding enc = new ASCIIEncoding();
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
                sw.Dispose();
                stream.Dispose();

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
                    info.callback(new ApiResult() { error = false, text = resultStr, errorText = "" });
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
                    callback(new ApiResult() { error = true, text = reader.ReadToEnd(), errorText = ex.Message });
            }
            else
            {
                if (callback != null)
                    callback(new ApiResult() { error = true, text = "No response.", errorText = ex.Message });
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