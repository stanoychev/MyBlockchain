using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BlockchainServer.Models
{
    [KnownType(typeof(BlockchainResponse))]
    [DataContract]
    public class BlockchainResponse
    {
        public BlockchainResponse(IEnumerable<Block> chain) => Chain = chain;

        [DataMember]
        public IEnumerable<Block> Chain { get; set; }
    }
}