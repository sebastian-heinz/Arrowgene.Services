/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Arrowgene.Services.Network.Http.Client
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Net.Security;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Creates a <see cref="HttpWebRequest"/> with configureable default values.
    /// </summary>
    public class HttpRequest
    {
        private Thread asyncHttpResponseThread;
        public event EventHandler<AsyncHttpResponseEventArgs> AsyncHttpResponse;

        public const HttpStatusCode NO_HTTP_STATUS_CODE_AVAILABLE = (HttpStatusCode) 0;

        public const string GET_METHOD = "GET";
        public const string HEAD_METHOD = "HEAD";
        public const string POST_METHOD = "POST";
        public const string PUT_METHOD = "PUT";
        public const string DELETE_METHOD = "DELETE";
        public const string TRACE_METHOD = "TRACE";
        public const string OPTIONS_METHOD = "OPTIONS";

        private string responseCharacterSet;

        public HttpRequest()
        {
            this.Reset();
            this.ResetReturnProperties();
        }

        public bool AllowAutoRedirect { get; set; }
        public bool PreAuthenticate { get; set; }
        public bool KeepAlive { get; set; }

        public byte[] PostPayload { get; set; }

        /// <summary>
        /// Size of read buffer.
        /// Bigger size results in fewer copy actions,
        /// but increases memory allocation.
        /// </summary>
        public uint BufferSize { get; set; }

        /// <summary>
        /// Timeout in ms to wait till a http response arrives.
        /// </summary>
        public int Timeout { get; set; }

        public int ReadWriteTimeout { get; set; }
        public string UserAgent { get; set; }

        /// <summary>
        /// Exception message of the last request.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// URL of the last request.
        /// Available after a request.
        /// </summary>
        public string RequestUrl { get; private set; }

        public string Method { get; set; }
        public string Accept { get; set; }
        public IWebProxy Proxy { get; set; }
        public AuthenticationLevel AuthenticationLevel { get; set; }
        public HttpStatusCode StatusCode { get; private set; }
        public WebExceptionStatus Status { get; private set; }
        public NetworkCredential NetworkCredential { get; set; }
        public BindIPEndPoint BindIpEndPointDelegate { get; set; }
        public WebHeaderCollection RequestHeaders { get; set; }

        /// <summary>
        /// Headers of the last response.
        /// Available after a request.
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; private set; }

        public override string ToString()
        {
            string headers = string.Empty;

            foreach (string key in this.RequestHeaders.AllKeys)
            {
                headers += key + " " + this.RequestHeaders.Get(key) + "; ";
            }

            return string.Format("URL:{0}\r\n Method:{1}\r\n StatusCode:{2}\r\n Timeout:{3}\r\n ReadWriteTimeout:{4}\r\n Headers:{5}", this.RequestUrl, this.Method, this.StatusCode, this.Timeout, this.ReadWriteTimeout,
                headers);
        }

        public void SetCredential(string userName, string password)
        {
            this.NetworkCredential = new NetworkCredential(userName, password);
        }

        public void SetBasicAuthenticationHeader(string userName, string password)
        {
            string authentication = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password));
            this.RequestHeaders.Add("Authorization", authentication);
        }

        public void AddHeader(string name, string value)
        {
            this.RequestHeaders.Add(name, value);
        }

        public string RequestContent(string url)
        {
            string page = string.Empty;

            byte[] response = this.Request(url);

            if (response != null)
            {
                if (string.IsNullOrEmpty(this.responseCharacterSet))
                {
                    page = Encoding.UTF8.GetString(response);
                }
                else
                {
                    page = Encoding.GetEncoding(this.responseCharacterSet).GetString(response);
                }
            }

            return page;
        }

        public void RequestAsync(string url)
        {
            this.RequestUrl = url;

            this.asyncHttpResponseThread = new Thread(this.RequestAsync);
            this.asyncHttpResponseThread.Name = "AsyncHttpRequest (" + this.RequestUrl + ")";
            this.asyncHttpResponseThread.Start();
        }

        public byte[] Request(string url)
        {
            byte[] response = null;
            this.RequestUrl = url;

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest) WebRequest.Create(url);
                httpRequest.Proxy = this.Proxy;
                httpRequest.Timeout = this.Timeout;
                httpRequest.ReadWriteTimeout = this.ReadWriteTimeout;
                httpRequest.ServicePoint.BindIPEndPointDelegate = this.BindIpEndPointDelegate;
                httpRequest.UserAgent = this.UserAgent;
                httpRequest.Headers = this.RequestHeaders;
                httpRequest.AllowAutoRedirect = this.AllowAutoRedirect;
                httpRequest.PreAuthenticate = this.PreAuthenticate;
                httpRequest.AuthenticationLevel = this.AuthenticationLevel;
                httpRequest.KeepAlive = this.KeepAlive;
                httpRequest.Method = this.Method;
                httpRequest.Accept = this.Accept;

                if (this.NetworkCredential != null)
                {
                    httpRequest.Credentials = this.NetworkCredential;
                }

                if (this.PostPayload != null)
                {
                    this.WritePostPayload(httpRequest);
                }

                this.ResetReturnProperties();

                HttpWebResponse httpResponse = (HttpWebResponse) httpRequest.GetResponse();

                this.StatusCode = httpResponse.StatusCode;
                this.responseCharacterSet = httpResponse.CharacterSet;

                response = this.ReadResponse(httpResponse);
            }
            catch (WebException webException)
            {
                this.ExceptionMessage = webException.Message;
                this.Status = webException.Status;

                if (webException.Status == WebExceptionStatus.ProtocolError && webException.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse) webException.Response;
                    response = this.ReadResponse(webResponse);
                    this.StatusCode = webResponse.StatusCode;
                }
                else
                {
                    Debug.WriteLine(string.Format("HttpRequest::Request: {0} \r\n Error:{1}", this.ToString(), webException.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.ExceptionMessage = ex.Message;
                this.Status = WebExceptionStatus.UnknownError;
                Debug.WriteLine(string.Format("HttpRequest::Request: {0} \r\n Error:{1}", this.ToString(), ex.ToString()));
            }

            return response;
        }

        private void WritePostPayload(HttpWebRequest httpRequest)
        {
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = this.PostPayload.Length;

            using (Stream stream = httpRequest.GetRequestStream())
            {
                stream.Write(this.PostPayload, 0, this.PostPayload.Length);
            }
        }

        private byte[] ReadResponse(HttpWebResponse httpResponse)
        {
            byte[] response = null;
            Stream responseStream = null;

            this.ResponseHeaders = httpResponse.Headers;

            string content_encoding = httpResponse.Headers.Get("Content-Encoding");
            if (!string.IsNullOrEmpty(content_encoding) && content_encoding == "gzip")
            {
                responseStream = new GZipStream(httpResponse.GetResponseStream(), CompressionMode.Decompress);
            }
            else
            {
                responseStream = httpResponse.GetResponseStream();
            }

            byte[] buffer = new byte[this.BufferSize];
            int read = 0;
            using (responseStream)
            {
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (response == null)
                    {
                        response = new byte[read];
                        Buffer.BlockCopy(buffer, 0, response, 0, read);
                    }
                    else
                    {
                        int newSize = response.Length + read;
                        byte[] new_response = new byte[newSize];

                        Buffer.BlockCopy(response, 0, new_response, 0, response.Length);
                        Buffer.BlockCopy(buffer, 0, new_response, response.Length, read);

                        response = new_response;
                    }
                }
            }
            return response;
        }

        public void Reset()
        {
            this.BufferSize = 2048;
            this.Timeout = 2000;
            this.ReadWriteTimeout = 2000;
            this.NetworkCredential = null;
            this.Proxy = null;
            this.BindIpEndPointDelegate = null;
            this.RequestHeaders = new WebHeaderCollection();
            this.AllowAutoRedirect = false;
            this.PreAuthenticate = false;
            this.AuthenticationLevel = AuthenticationLevel.None;
            this.KeepAlive = false;
            this.PostPayload = null;
            this.Method = GET_METHOD;
        }

        private void ResetReturnProperties()
        {
            this.Status = WebExceptionStatus.Success;
            this.StatusCode = NO_HTTP_STATUS_CODE_AVAILABLE;
        }

        private void RequestAsync()
        {
            byte[] response = this.Request(this.RequestUrl);
            this.OnAsyncHttpResponse(response);
        }

        private void OnAsyncHttpResponse(byte[] response)
        {
            EventHandler<AsyncHttpResponseEventArgs> asyncHttpResponse = this.AsyncHttpResponse;
            if (asyncHttpResponse != null)
            {
                AsyncHttpResponseEventArgs asyncHttpResponseEventArgs = new AsyncHttpResponseEventArgs(response);
                asyncHttpResponse(this, asyncHttpResponseEventArgs);
            }
        }
    }
}