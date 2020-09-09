using System.Threading;

namespace BlockchainServer.HttpServer
{
    public class HttpServerThreadHolder
    {
        private readonly HttpServer httpServer;
        private readonly Thread serverThread;

        public HttpServerThreadHolder(HttpServer httpServer)
        {
            this.httpServer = httpServer;
            serverThread = new Thread(new ThreadStart(httpServer.Listen));
        }

        public void Start() => serverThread.Start();

        public void Dispose() => httpServer.Dispose();
    }
}