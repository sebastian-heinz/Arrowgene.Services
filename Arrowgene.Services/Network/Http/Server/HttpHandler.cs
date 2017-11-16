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

namespace Arrowgene.Services.Network.Http.Server
{
    using System;
    using System.IO;
    using System.Net;

    public abstract class HttpHandler
    {
        public HttpHandler()
        {
        }

        public abstract void Handle(HttpListenerContext context);

        protected void SendResponse(string input, HttpListenerContext context)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            this.SendResponse(stream, context);
        }

        protected void SendResponse(Stream input, HttpListenerContext context)
        {
            try
            {
                context.Response.ContentType = "text/xml";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                }
                input.Close();
                context.Response.OutputStream.Flush();
                context.Response.StatusCode = (int) HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                // Logger.Write("FakerModule::SendResponse", ex.ToString(), LogType.EXCEPTION);
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            context.Response.OutputStream.Close();
        }
    }
}