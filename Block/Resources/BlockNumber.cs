using System;
using System.Text;

namespace BlobUploadTool.Block.Resources
{
		public class BlockNumber
		{
				public BlockNumber()
				{
						_blockNumber = 1;
				}

				public string GetBlockId()
				{
						var textBlockId = $"{_blockNumber:0000000}";

						var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(textBlockId));

						_blockNumber++;

						return blockId;
				}

				public int GetBlockNumber()
				{
						return _blockNumber;
				}

				private int _blockNumber;
		}
}
