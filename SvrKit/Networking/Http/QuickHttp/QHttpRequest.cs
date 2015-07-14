namespace SvrKit.Networking.Http.QuickHttp
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

    public class QHttpRequest
    {
        private Thread thread;
        private string requestUrl;

        public QHttpRequest(string url)
        {
            this.requestUrl = url;
            this.Timeout = 1000;
            this.AcceptLanguage = "en-US";
            this.UserAgent = "SvrKit";
            this.ContentType = "text/html";
        }

        public EventHandler<QHttpRequestAsyncResponse> AsyncResponse;

        public int Timeout { get; set; }
        public string UserAgent { get; set; }
        public string AcceptLanguage { get; set; }
        public string ContentType { get; set; }

        /// <summary>
        /// Starts a Blocking WebRequest
        /// </summary>
        /// <returns></returns>
        public string Request()
        {
            HttpWebRequest request = this.CreateWebRequest(this.requestUrl);
            return this.ReadResponse(request);
        }

        /// <summary>
        /// Starts a new Thread to receive the response.
        /// Subscripe to AsyncResponse in order to get the result.
        /// </summary>
        /// <param name="url"></param>
        public void RequestAsync()
        {
            this.thread = new Thread(ReadResponseAsync);
            this.thread.Name = "WebRequest (" + this.requestUrl + ")";
            this.thread.Start();
        }

        private HttpWebRequest CreateWebRequest(string url)
        {
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(url);

                request.Headers.Set("Method", "GET");
                request.Method = "GET";
                request.Headers.Set("Accept-Language", this.AcceptLanguage);
                request.ContentType = this.ContentType;
                request.UserAgent = this.UserAgent;
                request.Timeout = this.Timeout;
                request.ReadWriteTimeout = this.Timeout;
                request.Proxy = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("QHttpRequest::CreateWebRequest:" + ex.ToString());
                request = null;
            }

            return request;
        }

        private void OnAsyncResponse(string response)
        {
            if (this.AsyncResponse != null)
            {
                QHttpRequestAsyncResponse handle = new QHttpRequestAsyncResponse(response);
                this.AsyncResponse(this, handle);
            }
        }

        private void ReadResponseAsync()
        {
            HttpWebRequest request = this.CreateWebRequest(this.requestUrl);
            string response = this.ReadResponse(request);
            this.OnAsyncResponse(response);
        }

        private string ReadResponse(HttpWebRequest request)
        {
            string responseText = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException webEx)
            {
                bool handled = false;

                if (webEx.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse resp = webEx.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        Debug.WriteLine("QHttpRequest::ReadResponse" + String.Format("Web Error: {0} ({1})", resp.StatusCode, request.RequestUri.AbsoluteUri));
                        handled = true;
                    }
                }

                if (!handled)
                {
                    Debug.WriteLine("QHttpRequest::ReadResponse", webEx.ToString());
                }

                request = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("QHttpRequest::ReadResponse", ex.ToString());
            }
            return responseText;
        }

    }
}