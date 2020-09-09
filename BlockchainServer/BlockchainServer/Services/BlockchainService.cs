using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using BlockchainServer.Models;

namespace BlockchainServer.Services
{
    public static class BlockchainService
    {
        private const string difficulty = "000";

        public static List<Block> InitializeBlockchain()
        {
            var emptyChain = new List<Block>();
            var genesisBlock = CreateGenesisBlock(emptyChain);
            return AddBlockToChain(emptyChain, genesisBlock);
        }

        public static Block CreateGenesisBlock(List<Block> chain)
            => CreateBlock(chain, 1, "0");

        public static Block CreateBlock(List<Block> chain, int nonce, string previousHash)
        {
            var block = new Block()
            {
                Id = "Block #" + (chain.Count + 1).ToString(),
                Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff"),
                Nonce = nonce,
                PreviousHash = previousHash
            };

            return block;
        }

        public static List<Block> AddBlockToChain(List<Block> chain, Block block)
        {
            chain.Add(block);
            return chain;
        }

        public static Block GetPrevious(List<Block> chain) 
            => chain.Last();

        public static string HashNonces(int previousNonce, int currentNonce)
            => Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.Default.GetBytes("b" + (Math.Sqrt(currentNonce) - Math.Sqrt(previousNonce)).ToString())));

        public static string HashBlock(Block block)
        {
            var settings = new DataContractJsonSerializerSettings();
            var js = new DataContractJsonSerializer(typeof(Block), settings);

            var ms = new MemoryStream();
            js.WriteObject(ms, block);
            ms.Position = 0;
            var sr = new StreamReader(ms);

            var encoded = "b" + sr.ReadToEnd();

            sr.Close();
            ms.Close();

            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.Default.GetBytes(encoded)));
        }

        public static int CalculateNonce(int previousNonce)
        {
            var nonce = 1;

            while (true)
            {
                var hashed = HashNonces(previousNonce, nonce);

                if (hashed.StartsWith(difficulty))
                    return nonce;
                else
                    nonce++;
            }
        }

        public static bool IsChainValid(List<Block> chain)
        {
            if (!chain.Any())
                return false;

            var previousBlock = chain[0];
            var blockIndex = 1;

            while (blockIndex < chain.Count)
            {
                var currentBlock = chain[blockIndex];
                if (currentBlock.PreviousHash != HashBlock(previousBlock))
                    return false;

                var hashed = HashNonces(previousBlock.Nonce, currentBlock.Nonce);
                if (!hashed.StartsWith(difficulty))
                    return false;

                previousBlock = currentBlock;
                blockIndex++;
            }

            return true;
        }
    }
}