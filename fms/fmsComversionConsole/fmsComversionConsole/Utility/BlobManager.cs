using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System.IO;
using System.Linq;


namespace fms
{
    public class BlobManager
    {
        private BlobClientOptions clientOptions;
        private BlobServiceClient blobServiceClient;

        public BlobManager()
        {
            SetDefaultOptions();
        }

        public BlobManager(string blobConnectionString, BlobClientOptions options = null)
        {
            if (options != null)
            {
                this.clientOptions = options;
            }
            else
            {
                SetDefaultOptions();
            }
            
            this.blobServiceClient = new BlobServiceClient(blobConnectionString, clientOptions);
            
            ;
        }

        private void SetDefaultOptions()
        {
            clientOptions = new BlobClientOptions
            {
                Retry = {
                    Delay = TimeSpan.FromSeconds(2),     //The delay between retry attempts for a fixed approach or the delay on which to base 
                    MaxRetries = 3,
                    Mode = RetryMode.Fixed,      
                    MaxDelay = TimeSpan.FromSeconds(8)
                }
            };
        }

        public async Task CreateBlobContainer(string containerName,string encryptionScope=null)
        {
            BlobContainerEncryptionScopeOptions scope = null;
            if (encryptionScope != null)
            {
                scope = new BlobContainerEncryptionScopeOptions()
                {
                    DefaultEncryptionScope = encryptionScope,
                    PreventEncryptionScopeOverride = true
                };
            }

            // Get and Create the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, null, scope);
        }

        public async Task<long> GetBlobFileLengthAsync(string containerName, string targetBlobName)
        {
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = container.GetBlobClient(targetBlobName);
            var blobExists = await blobClient.ExistsAsync();
            if(blobExists)
            {
                var properties = (await blobClient.GetPropertiesAsync()).Value;
                var len = properties.ContentLength;
                return len;
            }
            return -1;
        }

        public async Task<BlobContentInfo> UploadBlobStreamAsync(string containerName, string targetBlobName, Stream sourceStream, string contentType)
        {
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, null);
            var blobClient = container.GetBlobClient(targetBlobName);

            var uploadOptions = new BlobUploadOptions();
            if (contentType != null) 
            {
                uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };
            }

            var blobExists = await blobClient.ExistsAsync();
            if (!blobExists)
            {
                var response = await blobClient.UploadAsync(sourceStream, uploadOptions);
                return response;
            }

            return null;
        }

        private static Uri GetServiceSasUriForBlob(BlobClient blobClient,
            string storedPolicyName = null)
        {
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(2);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read |
                                              BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri;
            }
            else
            {
                throw new Exception(@"Authoriztion issue");
            }
        }
        
        public List<string> ListBlobNamesOrdered(string containerName, string folderName = null, string fileExtension = null)
        {
            List<string> blobNames = null;
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            bool exists = blobContainerClient.Exists();
            if (exists)
            {
                blobNames = new List<string>();
                var blobItems = blobContainerClient.GetBlobs(prefix: folderName);
                foreach (BlobItem blobItem in blobItems)
                {
                    if(fileExtension == null || blobItem.Name.ToLowerInvariant().EndsWith(fileExtension.ToLowerInvariant()))
                    {
                        blobNames.Add(blobItem.Name);
                    }
                }
            }

            blobNames = blobNames?.OrderBy(x => x).ToList();
            return blobNames;
        }

        public static List<string> ListMP4BlobWithSASUrlsOrdered(List<string> blobConnectionStrings, string containerName, string folderName = null)
        {
            var urls = new List<string>();
            foreach (var blobConnectionString in blobConnectionStrings)
            {
                var blobService = new BlobManager(blobConnectionString);

                List<string> blobNames = blobService.ListBlobNamesOrdered(containerName, folderName);
                if (blobNames != null)
                {
                    foreach (var blobName in blobNames)
                    {
                        if (blobName.EndsWith("mp4"))
                            urls.Add(blobService.GetBlobURL( containerName, blobName).AbsoluteUri);
                    }
                    break;
                }
            }
            
            return urls;
        }

        public Uri GetBlobURL(string containerName, string blobName)
        {
            var blob = GetBlobClient(containerName, blobName);
            return GetServiceSasUriForBlob(blob);
        }

        public Stream GetBlobStream(string containerName, string blobName, string folderName = null)
        {
            var path = folderName==null?blobName:folderName + "/" + blobName;
            var blob = GetBlobClient(containerName, path);
            return  blob.OpenRead();
        }


        private BlobClient GetBlobClient(string containerName, string blobName)
        {
            blobName = blobName.Replace("\\", "/");
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            if (!container.Exists())
                throw new ArgumentException($"Container {container} does not exist.");

            var blob = container.GetBlobClient(blobName);
            if (!blob.Exists())
                throw new ArgumentException($"Blob {blobName} does not exist.");

            return blob;
        }

        public async Task<int> CopyBlobContainerInSameStorage(string sourceContainerName, string destContainerName)
        {
            BlobContainerClient sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
            BlobContainerClient destContainerClient = blobServiceClient.GetBlobContainerClient(destContainerName);

            await destContainerClient.CreateAsync();

            var blobItems = sourceContainerClient.GetBlobsAsync().GetAsyncEnumerator();

            int count = 0;
            while (await blobItems.MoveNextAsync())
            {
                BlobItem blobItem = blobItems.Current;
                BlobClient sourceBlobClient = sourceContainerClient.GetBlobClient(blobItem.Name);
                BlobClient destBlobClient = destContainerClient.GetBlobClient(blobItem.Name);

                await destBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
                count++;
            }

            return count;
        }

        public async Task<bool> BlobContainerExists(string containerName)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            return (await containerClient.ExistsAsync());
        }

        public async Task DeleteBlobContainer(string containerName, bool waitAfterDeletion, int waitTime = 10)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            if (await containerClient.ExistsAsync())
            {
                await containerClient.DeleteAsync();
                if(waitAfterDeletion)
                    await Task.Delay(waitTime * 1000);
            }
            else
                Console.WriteLine($"Container '{containerName}' does not exist.");
        }
    }
}
