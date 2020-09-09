using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BlockchainServer.Models;
using BlockchainServer.Services;

namespace BlockchainServer.HttpServer
{
    public class HttpProcessor
    {
        private const string getchainPathString = "/get";
        private const string mineblockPathString = "/mine";
        private const string textTypeResponseString = "text/plain";
        private const string jsonTypeResponseString = "Application/json";

        private readonly string securityHeader;
        private readonly string allowedCORSOrigin;

        public HttpProcessor(string allowedCORSOrigin, string securityHeader)
        {
            this.allowedCORSOrigin = allowedCORSOrigin;
            this.securityHeader = securityHeader;
        }

        public void HandleHttpClient(IAsyncResult result)
        {
            var context = (result.AsyncState as HttpListener).EndGetContext(result);

            //if (context.Request.HttpMethod != "OPTIONS" && !context.Request.Headers.AllKeys.Contains(securityHeader))
            //{
            //    PostProcessRequest(context, HttpStatusCode.BadRequest, textTypeResponseString, "Security header not found.");
            //    return;
            //}

            switch (context.Request.HttpMethod)
            {
                //case "PUT": ProcessPutRequest(context); break;
                //case "DELETE": ProcessDeleteRequest(context); break;
                case "GET": ProcessGetRequest(context); break;
                case "OPTIONS": ProcessOptionsRequest(context); break;
                default:
                    PostProcessRequest(context, HttpStatusCode.BadRequest, textTypeResponseString,
               $"Method {context.Request.HttpMethod} not expected.\nThe server accepts only PUT, DELETE or GET."); break;
            }
        }

        private T PreProcessRequest<T>(HttpListenerContext context)
        {
            var request = context.Request;
            var inputStream = request.InputStream;

            T preProcessedRequest = default;
            try
            {
                preProcessedRequest = JsonParser.Deserialize<T>(inputStream);
            }
            catch { }
            inputStream.Close();

            return preProcessedRequest;
        }

        //private void ProcessPutRequest(HttpListenerContext context)
        //{
        //    var putRequest = PreProcessRequest<PutRequest>(context);

        //    PostProcessRequest(context, HttpStatusCode.Ok, textTypeResponseString, $"{putRequest.RequesterId} updated.");
        //}

        //private void ProcessDeleteRequest(HttpListenerContext context)
        //{
        //    var deleteRequest = PreProcessRequest<DeleteRequest>(context);

        //    PostProcessRequest(context, HttpStatusCode.NotFound, textTypeResponseString, $"{deleteRequest.RequesterId} not found.");
        //}

        private void ProcessGetRequest(HttpListenerContext context)
        {
            #region validation
            var validateQuerryString = ValidateQuerryString(context);
            if (validateQuerryString != null)
            {
                PostProcessRequest(context, HttpStatusCode.BadRequest, textTypeResponseString, validateQuerryString);
                return;
            }
            #endregion

            string result = null;

            //try
            {
                if (context.Request.Url.LocalPath == getchainPathString)
                    result = JsonParser.Serialize(new BlockchainResponse(BlockchainHolder.Chain));
                else if (context.Request.Url.LocalPath == mineblockPathString)
                {
                    var previousBlock = BlockchainService.GetPrevious(BlockchainHolder.Chain);
                    var previousNonce = previousBlock.Nonce;
                    var newNonce = BlockchainService.CalculateNonce(previousNonce);
                    var previousHash = BlockchainService.HashBlock(previousBlock);
                    var block = BlockchainService.CreateBlock(BlockchainHolder.Chain, newNonce, previousHash);
                    BlockchainService.AddBlockToChain(BlockchainHolder.Chain, block);

                    result = JsonParser.Serialize(block);
                }

                if (!BlockchainService.IsChainValid(BlockchainHolder.Chain))
                    throw new ArgumentException("Invalid blockchain.");
            }
            //catch (Exception ex)
            {
                //result = $"{{ "Exception" : \"{ex.Message}\"}}";
            }

            PostProcessRequest(context, HttpStatusCode.Ok, jsonTypeResponseString, result);
        }

        private void ProcessOptionsRequest(HttpListenerContext context)
        {
            var response = context.Response;

            response.Headers.Add("Access-Control-Allow-Methods: OPTIONS, PUT, DELETE, GET");
            response.Headers.Add($"Access-Control-Allow-Headers: {securityHeader}");

            PostProcessRequest(context, HttpStatusCode.Ok, textTypeResponseString, string.Empty);
        }

        private string ValidateQuerryString(HttpListenerContext context)
        {
            var localPath = context.Request.Url.LocalPath.ToLower();
            if (localPath != getchainPathString || localPath != mineblockPathString)
                return null;

            return "Method not implemented.";
        }

        private void PostProcessRequest(HttpListenerContext context, HttpStatusCode statusCode, string contentType, string message)
        {
            var response = context.Response;

            response.ContentType = contentType;
            response.ContentEncoding = context.Request.ContentEncoding;
            response.StatusCode = (int)statusCode;

            response.Headers.Add($"Access-Control-Allow-Origin: {allowedCORSOrigin}");

            var outputStream = response.OutputStream;
            Write(outputStream, message, context.Request.ContentEncoding);
            outputStream.Flush();
            outputStream.Close();
        }

        private void Write(Stream stream, string message, Encoding encoding)
        {
            var bytes = encoding.GetBytes(message);
            stream.Write(bytes, 0, bytes.Length);
        }

        private enum HttpStatusCode
        {
            Ok = 200,
            Created = 201,
            NoContent = 204,
            BadRequest = 400,
            NotFound = 404
        }
    }
}