using System.Runtime.Serialization;

namespace BlockchainServer.Models
{
    [DataContract]
    public class Block
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Timestamp { get; set; }
        [DataMember]
        public int Nonce { get; set; }
        [DataMember]
        public string PreviousHash { get; set; }
        //[DataMember]
        //public string CurrentHash { get; set; }
        [DataMember]
        public string Data { get; set; }
    }
}