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
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
               // Logger.Write("FakerModule::SendResponse", ex.ToString(), LogType.EXCEPTION);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.OutputStream.Close();
        }

    }
}
