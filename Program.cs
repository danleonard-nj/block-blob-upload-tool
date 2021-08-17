using Azure.Storage.Blobs;
using BlobUploadTool.Block;
using System;
using System.IO;
using System.Linq;

namespace BlobUploadTool
{
		class Program
		{
				static void Main(string[] args)
				{
						var storageConnectionString = "";

						var directory = @"";

						var blockBlobUpload = new BlockComponent(storageConnectionString, "podcasts", directory, blockSize: 8000000);

						blockBlobUpload.UploadConcurrent(8).Wait();
				}
		}
}
