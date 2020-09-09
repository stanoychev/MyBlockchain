using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace BlockchainServer.Services
{
    public static class JsonParser
    {
        private readonly static DataContractJsonSerializerSettings settings
            = new DataContractJsonSerializerSettings() { EmitTypeInformation = EmitTypeInformation.Never };

        public static string Serialize<T>(T obj)
        {
            var memoryStream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(T), settings);
            serializer.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            var result = streamReader.ReadToEnd();
            memoryStream.Dispose();
            streamReader.Dispose();

            return result;
        }

        public static T Deserialize<T>(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(T), settings);
            return (T)serializer.ReadObject(stream);
        }
    }
}