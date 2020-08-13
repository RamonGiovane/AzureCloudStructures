using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
///     2020 - Version 1.4.0 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.CloudStructures
{
    /// <summary>
    ///  Manipulates a blob container from the Azure storage.
    ///  An instance of this class can only handle one container, which must be specified by it's name on the constructor.
    ///  With this class is possible to create, load or delete a container, and upload to, download or delete binary data from a container.
    /// </summary>
    public class AzureBlobs : CloudStructure
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobs(string containerName) : this(containerName, null) { }

        public AzureBlobs(string containerName, ILogger logger) : base(containerName, logger)
        {
            _blobServiceClient = new BlobServiceClient(ConnectionString);
        }

        public AzureBlobs(string containerName, string connectionString, ILogger logger) : base(containerName, connectionString, logger)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public override bool CreateOrLoadStructure()
        {
            if (!IsCreated())
            {

                return LogAzureResponse("Create Container", Task.Run(
                    () => _blobServiceClient.CreateBlobContainerAsync(StructureName)).Result.GetRawResponse());
            }
            return true;
        }

        public override bool IsCreated()
        {

            return Task.Run(() => _blobServiceClient.GetBlobContainerClient(StructureName).ExistsAsync()).Result;

        }

        private BlobClient CreateBlobObject(string blobName)
        {
            return new BlobClient(ConnectionString, StructureName, blobName);
        }

        public override bool DeleteStructure()
        {
            if (!IsCreated()) return false;
            return LogAzureResponse("Delete Container",
                Task.Run(() => _blobServiceClient.GetBlobContainerClient(StructureName).DeleteAsync()).Result);
        }


        /// <summary>
        ///     Uploads a local file from the current file system to the cloud blob container.
        ///     <br></br>
        ///     If the parameter <paramref name="fileNameOnStorage"/> is negligenciated, it will be assumed as the same value
        ///     of the given <paramref name="filePath"/>.
        ///     <br></br><br></br>
        ///     
        ///     
        /// 
        ///     Warning: using the default value of <paramref name="fileNameOnStorage"/> and an absolute path in <paramref name="filePath"/>
        ///     can be dangerous.
        ///     <br></br><br></br>
        ///     Warning: using a relative path in <paramref name="filePath"/> can be dangerous while executing in the Azure environment
        ///     
        ///     
        /// </summary>
        /// <param name="filePath">The file to be uploaded from the current file system.</param>
        /// <returns></returns>
        /// <param name="stream">The file to be uploaded as stream</param>
        /// <param name="fileNameOnStorage">The name of the new file to be uploaded in the container</param>
        public async Task<bool> UploadBlob(string filePath, string fileNameOnStorage = null)
        {
            try
            {
                var blob = CreateBlobObject(fileNameOnStorage ?? filePath);

                if (await blob.ExistsAsync())
                {
                    TryLog($"{StructureName}: {fileNameOnStorage ?? filePath} already exists.");
                    return true;
                }

                var response = await blob.UploadAsync(filePath);

                return LogAzureResponse("Upload Blob", response.GetRawResponse());
            }
            catch (Exception e)
            {
                TryLogError($"{StructureName}: Error attempting to upload: {fileNameOnStorage} - {e.Message}");
                return false;
            }

        }


        /// <summary>
        ///     Uploads a local file from the current file system to the cloud blob container.
        ///     <br></br><br></br>
        ///     Warning: using relative filepaths to file to be uploaded can be dangerous while executing in the Azure environment.
        /// </summary>
        /// <returns></returns>
        /// <param name="stream">The file to be uploaded as stream</param>
        /// <param name="fileNameOnStorage">The name of the new file to be uploaded in the container</param>
        /// <returns></returns>
        public async Task<bool> UploadBlob(Stream stream, string fileNameOnStorage)
        {
            try
            {
                var blobClient = CreateBlobObject(fileNameOnStorage);

                if (await blobClient.ExistsAsync())
                {
                    TryLog($"{StructureName}: {fileNameOnStorage} already exists.");
                    return true;
                }

                var response = (await blobClient.UploadAsync(stream)).GetRawResponse();

                return LogAzureResponse("Upload Blob", response);
            }
            catch (Exception e)
            {
                TryLogError($"{StructureName}: Error attempting to upload: {fileNameOnStorage} - {e.Message}");
                return false;
            }

        }

        public async Task<MemoryStream> DownloadBlobAsStream(string blobName)
        {
            var blob = CreateBlobObject(blobName);
            var ms = new MemoryStream();

            try
            {
                LogAzureResponse("Download Blob", await blob.DownloadToAsync(ms));

                ms.Position = 0;

                return ms;
            }
            catch (Exception e)
            {
                TryLogError($"{StructureName}: Error attempting to download: {blobName} - {e.Message}");
                return null;
            }
        }


        /// <summary>
        ///     Donwloads a blob file from the container and saves it in a local file in the current file system.
        ///     <br></br><br></br>
        ///     Warning: using relative filepaths to new file to be created can be dangerous while executing in the Azure environment.
        /// </summary>
        /// <param name="blobName">The name of the file in the container</param>
        /// <param name="filePathLocally">The full name (filepath) of the file to be created on the current file system.</param>
        /// <returns></returns>
        public async Task<string> DownloadBlobAsFile(string blobName, string filePathLocally)
        {
            try
            {
                var blob = CreateBlobObject(blobName);

                if (LogAzureResponse("Download Blob to File", await blob.DownloadToAsync(filePathLocally)))
                    return filePathLocally;
            }
            catch (Exception e)
            {
                TryLogError($"{StructureName}: Error attempting to download: {blobName} - {e.Message}");
            }
            return null;
        }

        public bool DeleteBlob(string blobName)
        {
            return LogAzureResponse("Delete Blob", CreateBlobObject(blobName).Delete());
        }

        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            return LogAzureResponse("Delete Blob", await CreateBlobObject(blobName).DeleteAsync());
        }


    }
}