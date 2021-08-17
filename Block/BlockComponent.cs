using BlobUploadTool.Block.Resources;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlobUploadTool.Utilities;

namespace BlobUploadTool.Block
{
		public class BlockComponent
		{
				public BlockComponent(
						string connectionString,
						string containerName,
						string directory,
						string fileExtensions = "*.mp3",
						int blockSize = 8000000)
				{
						_files = GetFiles(directory, fileExtensions);
						_containerClient = GetBlobContainerClient(connectionString, containerName);
						_blockSize = blockSize;

						//_files = _files.Take(6);
				}

				private CloudBlobContainer GetBlobContainerClient(string connectionString, string containerName)
				{
						var storageAccount = CloudStorageAccount.Parse(connectionString);

						var blobClient = storageAccount.CreateCloudBlobClient();
						var blobContainerClient = blobClient.GetContainerReference(containerName);

						return blobContainerClient;
				}

				private IEnumerable<FileInfo> GetFiles(string directory, string fileExtension)
				{
						var directoryInfo = new DirectoryInfo(directory);
						var files = directoryInfo.GetFiles(fileExtension);

						return files;
				}

				public async Task<IEnumerable<BlockSegment>> GetFileStreamBlocks(FileInfo singleFile)
				{
						ConsoleWriter.WriteBlue($"{GetType()}: {singleFile.Name}: Creating block segments.");

						int bytesRead;

						var blockNumber = new BlockNumber();

						var blocks = new List<BlockSegment>();

						using (var stream = File.OpenRead(singleFile.FullName))
						{
								var totalBlocks = stream.Length / _blockSize;

								ConsoleWriter.WriteBlue($"{GetType()}: {singleFile.Name}: {stream.Length / _blockSize} block segments queued.");

								do
								{

										var buffer = new byte[_blockSize];
										bytesRead = await stream.ReadAsync(buffer, 0, _blockSize);

										var block = new BlockSegment
										{
												BlockNumber = blockNumber.GetBlockNumber(),
												BlockId = blockNumber.GetBlockId(),
												Content = new MemoryStream(buffer, 0, bytesRead)
										};

										blocks.Add(block);

								} while (bytesRead == _blockSize); 
						}

						ConsoleWriter.WriteGreen($"{GetType()}: {singleFile.Name}: Block segments created successfully!");

						return blocks;
				}

				public async Task UploadSingle(FileInfo singleFile, string blobName)
				{
						var blocks = await GetFileStreamBlocks(singleFile);
						var blockCount = blocks.Count();

						ConsoleWriter.WriteBlue($"{GetType()}: {singleFile.Name}: Uploading {blockCount} blocks.");

						var blob = _containerClient.GetBlockBlobReference(blobName);

						using (var semaphore = new SemaphoreSlim(8, 50))
						{
								var blockTasks = blocks.Select(async block =>
								{
										await semaphore.WaitAsync();

										try
										{
												//ConsoleWriter.WriteWhite($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: Uploading.");
												

												await blob.PutBlockAsync(block.BlockId, block.Content, default);
										}

										catch (Exception ex)
										{
												ConsoleWriter.WriteRed($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: Failed to upload block.");
												ConsoleWriter.WriteRed($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: {ex.GetType().FullName}");
												ConsoleWriter.WriteRed($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: {ex.Message}");

												throw new Exception($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: Upload halted.");
										}

										finally
										{
												//ConsoleWriter.WriteGreen($"{GetType()}: {singleFile.Name}: Block #{block.BlockNumber}: Upload successful!");

												block.Dispose();

												semaphore.Release();
										}
								});

								await Task.WhenAll(blockTasks);

								var blockList = blocks.Select(x => x.BlockId);

								await blob.PutBlockListAsync(blockList);

								ConsoleWriter.WriteGreen($"{GetType()}: {singleFile.Name}: Blob uploaded successfully!");
						}
				}

				public async Task UploadConcurrent(int maxConcurrency)
				{
						ConsoleWriter.WriteBlue($"{GetType()}: {_files.Count()} files queued for upload.");

						using (var semaphore = new SemaphoreSlim(maxConcurrency, 50))
						{
								var fileUploadTasks = _files.Select(async file =>
								{
										await semaphore.WaitAsync();

										try
										{
												ConsoleWriter.WriteBlue($"{GetType()}: {file.Name}: Upload routine initialized.");

												await UploadSingle(file, file.Name);
										}

										finally
										{
												ConsoleWriter.WriteBlue($"{GetType()}: {file.Name}: Blob uploaded successfully!");

												semaphore.Release();
										}
								});

								await Task.WhenAll(fileUploadTasks);
						}
				}

				public async Task TestRun()
				{
						var singleFile = _files.FirstOrDefault();

						await UploadSingle(singleFile, $"test-block-{Guid.NewGuid()}");
				}

				private readonly CloudBlobContainer _containerClient;
				private readonly IEnumerable<FileInfo> _files;
				private int _blockSize;
		}
}
