using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ABCRetail.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            var containerName =
                configuration["AzureStorage:BlobContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "Blob container name is missing.");
            }

            _containerClient =
                new BlobContainerClient(
                    connectionString,
                    containerName);

            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            var fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            using var stream = image.OpenReadStream();

            var headers = new BlobHttpHeaders
            {
                ContentType = image.ContentType
            };

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = headers
                });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            var uri = new Uri(imageUrl);

            var blobName =
                Uri.UnescapeDataString(
                    uri.AbsolutePath.Split('/').Last());

            var blobClient =
                _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

        public string GetImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return string.Empty;
            }

            var uri = new Uri(imageUrl);

            var blobName = Uri.UnescapeDataString(
                uri.AbsolutePath.Split('/').Last());

            var blobClient = _containerClient.GetBlobClient(blobName);

            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerClient.Name,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(
                    BlobSasPermissions.Read);

                return blobClient
                    .GenerateSasUri(sasBuilder)
                    .ToString();
            }

            return imageUrl;
        }
    }
}