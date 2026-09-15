using ABCRetail.Functions.Models;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ABCRetail.Functions
{
    public class ProcessInventory
    {
        private readonly ILogger<ProcessInventory> _logger;

        public ProcessInventory(
            ILogger<ProcessInventory> logger)
        {
            _logger = logger;
        }

        [Function("ProcessInventory")]
        public async Task<HttpResponseData> Run([HttpTrigger( AuthorizationLevel.Function,"post")] HttpRequestData req)
        {
            _logger.LogInformation( "ProcessInventory function received a request.");

            try
            {
                // Read the inventory information sent by the web app.
                var message = await JsonSerializer
                        .DeserializeAsync<InventoryQueueMessage>(
                            req.Body,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (message == null)
                {
                    var badRequest =req.CreateResponse( HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync( "Inventory information is required.");
                    return badRequest;
                }

                // Get the Azure Storage connection string.
                var connectionString = Environment.GetEnvironmentVariable( "AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException( "Azure Storage connection string is not configured.");
                }

                // Get the inventory queue name.
                var inventoryQueueName = Environment.GetEnvironmentVariable( "InventoryQueueName");

                if (string.IsNullOrWhiteSpace(inventoryQueueName))
                {
                    throw new InvalidOperationException("Inventory queue name is not configured.");
                }

                // Connect to the existing inventory queue.
                var queueClient =
                    new QueueClient(
                        connectionString,
                        inventoryQueueName);

                await queueClient.CreateIfNotExistsAsync();

                // Convert the inventory message to JSON.
                var json =JsonSerializer.Serialize(message);

                // Send the message to Azure Queue Storage.
                await queueClient.SendMessageAsync(json);

                _logger.LogInformation("Inventory transaction {InventoryId} added to Azure Queue successfully.", message.InventoryId);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Inventory transaction sent successfully.",
                    inventoryId = message.InventoryId,
                    queue = inventoryQueueName
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError( ex,"Error sending inventory transaction to Azure Queue.");

                var response = req.CreateResponse( HttpStatusCode.InternalServerError);

                await response.WriteStringAsync("An error occurred while processing the inventory transaction.");

                return response;
            }
        }
    }
}