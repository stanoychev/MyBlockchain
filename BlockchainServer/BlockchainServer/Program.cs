using BlockchainServer.HttpServer;

namespace BlockchainServer
{
    public class Program
    {
        public static void Main()
        {
            var port = 5000;
            
            var httpProcessor = new HttpProcessor("*", "SecurityHeader");
            var httpServer = new HttpServer.HttpServer(port, httpProcessor);
            var serverThreadHolder = new HttpServerThreadHolder(httpServer);
            serverThreadHolder.Start();
        }
    }
}