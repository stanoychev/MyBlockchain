using System.Collections.Generic;
using BlockchainServer.Services;

namespace BlockchainServer.Models
{
    public static class BlockchainHolder
    {
        private static List<Block> chain;

        public static List<Block> Chain
        {
            get
            {
                if (chain == null)
                    chain = BlockchainService.InitializeBlockchain();

                return chain;
            }
        }
    }
}