using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace ABCRetail.Functions
{
    public class UploadProductImage
    {
        private readonly ILogger<UploadProductImage> _logger;

        public UploadProductImage(ILogger<UploadProductImage> logger)
        {
            _logger = logger;
        }

        [Function("UploadProductImage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestData req)
        {
            _logger.LogInformation("UploadProductImage function received a request.");

            try
            {
                // Read the request body
                using var reader = new StreamReader( req.Body, Encoding.UTF8);

                var fileName = req.Headers.TryGetValues(
                    "X-File-Name",
                    out var fileNameValues)
                    ? fileNameValues.FirstOrDefault()
                    : null;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync( "Please provide a file name using the X-File-Name header.");

                    return badRequest;
                }

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Azure Storage connection string is not configured.");
                }

                var containerClient = new BlobContainerClient(connectionString, "product-images");

                await containerClient.CreateIfNotExistsAsync();

                var blobName =  $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

                var blobClient =containerClient.GetBlobClient(blobName);

                //req.Body.Position = 0;

                await blobClient.UploadAsync(req.Body, overwrite: true);

                _logger.LogInformation( "File {FileName} uploaded successfully.", blobName);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Product image uploaded successfully.",
                    fileName = blobName,
                    container = "product-images",
                    url = blobClient.Uri.ToString()
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError( ex, "Error uploading product image to Azure Blob Storage.");

                var response =  req.CreateResponse( HttpStatusCode.InternalServerError);

                await response.WriteStringAsync( "An error occurred while uploading the product image.");

                return response;
            }
        }
    }
}