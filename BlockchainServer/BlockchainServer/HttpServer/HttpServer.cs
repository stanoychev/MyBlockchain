using System;
using System.Net;

namespace BlockchainServer.HttpServer
{
    public class HttpServer
    {
        private readonly int port;
        private readonly HttpListener listener = new HttpListener();
        private readonly HttpProcessor processor;
        private bool isActive = true;
        private IAsyncResult result;

        public HttpServer(int port, HttpProcessor processor)
        {
            this.port = port;
            this.processor = processor;
        }

        public void Listen()
        {
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            while (isActive)
            {
                result = listener.BeginGetContext(new AsyncCallback(processor.HandleHttpClient), listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        public void Dispose()
        {
            isActive = false;
            result.AsyncWaitHandle.Close();
            listener.Close();
        }
    }
}