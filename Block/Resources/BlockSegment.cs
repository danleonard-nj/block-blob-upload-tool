using System;
using System.IO;

namespace BlobUploadTool.Block.Resources
{
		public class BlockSegment : IDisposable
		{
				public int BlockNumber { get; set; }
				public string BlockId { get; set; }
				public Stream Content { get; set; }

				public void Dispose()
				{
						Content.Dispose();
				}
		}
}
