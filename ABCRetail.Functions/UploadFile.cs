using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ABCRetail.Functions
{
    public class UploadFile
    {
        private readonly ILogger<UploadFile> _logger;

        public UploadFile(ILogger<UploadFile> logger)
        {
            _logger = logger;
        }

        [Function("UploadFile")]
        public async Task<HttpResponseData> Run([HttpTrigger( AuthorizationLevel.Function, "post")]HttpRequestData req)
        {
            _logger.LogInformation( "UploadFile function received a request.");

            try
            {
                var requestData =
                    await JsonSerializer.DeserializeAsync<FileUploadRequest>(
                        req.Body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (requestData == null ||
                    string.IsNullOrWhiteSpace(requestData.FileName) ||
                    string.IsNullOrWhiteSpace(requestData.Content))
                {
                    var badRequest =req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync( "File name and content are required.");

                    return badRequest;
                }

                var connectionString =Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Azure Storage connection string is not configured.");
                }

                var fileShareName =
                    Environment.GetEnvironmentVariable(
                        "FileShareName");

                if (string.IsNullOrWhiteSpace(fileShareName))
                {
                    throw new InvalidOperationException(
                        "Azure File Share name is not configured.");
                }

                var shareClient =
                    new ShareClient(
                        connectionString,
                        fileShareName);

                await shareClient.CreateIfNotExistsAsync();

                var rootDirectory =shareClient.GetRootDirectoryClient();

                var fileClient = rootDirectory.GetFileClient( requestData.FileName);

                byte[] fileBytes = Encoding.UTF8.GetBytes( requestData.Content);

                using var stream = new MemoryStream(fileBytes);

                await fileClient.CreateAsync(
                    stream.Length);

                await fileClient.UploadRangeAsync(
                    new Azure.HttpRange(
                        0,
                        stream.Length),
                    stream);

                _logger.LogInformation("File {FileName} uploaded to Azure Files successfully.", requestData.FileName);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message ="File uploaded successfully.",
                    fileName =requestData.FileName,
                    fileShare = fileShareName
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error uploading file to Azure Files.");

                var response = req.CreateResponse( HttpStatusCode.InternalServerError);

                await response.WriteStringAsync( "An error occurred while uploading the file.");

                return response;
            }
        }
    }

    public class FileUploadRequest
    {
        public string FileName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}