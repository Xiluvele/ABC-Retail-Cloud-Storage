using ABCRetail.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class AzureFunctionService
    {
        private readonly HttpClient _httpClient;

        public AzureFunctionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> StoreCustomerAsync(Customer customer)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StoreCustomer",customer);

            return response.IsSuccessStatusCode;
        }

        public async Task<string?> UploadProductImageAsync(IFormFile image)
        {
            using var streamContent = new StreamContent(image.OpenReadStream());

            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);

            using var request = new HttpRequestMessage( HttpMethod.Post, "api/UploadProductImage");

            request.Headers.Add("X-File-Name", image.FileName);

            request.Content = streamContent;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (result.TryGetProperty("url", out var url))
            {
                return url.GetString();
            }

            return null;
        }

        // ============================================================
        // FUNCTION 3 - SEND ORDER TO AZURE QUEUE STORAGE
        // ============================================================

        public async Task<bool> SendOrderAsync(OrderQueueMessage message)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/ProcessOrder",
                    message);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendInventoryAsync(InventoryQueueMessage message)
        {
            var response = await _httpClient.PostAsJsonAsync(
                    "api/ProcessInventory",
                    message);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UploadFileAsync( string fileName,string content)
        {
            var request = new
            {
                FileName = fileName,
                Content = content
            };

            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/UploadFile",
                    request);

            return response.IsSuccessStatusCode;
        }
    }
}