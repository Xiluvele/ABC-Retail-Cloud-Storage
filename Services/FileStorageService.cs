using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Text;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            var fileShareName =
                configuration["AzureStorage:FileShareName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            if (string.IsNullOrWhiteSpace(fileShareName))
            {
                throw new InvalidOperationException(
                    "Azure File Share name is missing.");
            }

            _shareClient =new ShareClient( connectionString, fileShareName);

            _shareClient.CreateIfNotExists();
        }

        public async Task UploadLogAsync(string fileName,string content)
        {
            var rootDirectory =  _shareClient.GetRootDirectoryClient();

            var fileClient = rootDirectory.GetFileClient(fileName);

            byte[] fileBytes = Encoding.UTF8.GetBytes(content);

            using var stream = new MemoryStream(fileBytes);

            await fileClient.CreateAsync(stream.Length);

            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(
                    0,
                    stream.Length),
                stream);
        }

        public async Task<List<string>> GetFilesAsync()
        {
            var files = new List<string>();

            var rootDirectory =
                _shareClient.GetRootDirectoryClient();

            await foreach (
                ShareFileItem item in
                rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }

        public async Task<Stream?> DownloadFileAsync(
            string fileName)
        {
            var rootDirectory =  _shareClient.GetRootDirectoryClient();

            var fileClient = rootDirectory.GetFileClient(fileName);

            if (!await fileClient.ExistsAsync())
            {
                return null;
            }

            var download =
                await fileClient.DownloadAsync();

            var memoryStream =
                new MemoryStream();

            await download.Value.Content
                .CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}