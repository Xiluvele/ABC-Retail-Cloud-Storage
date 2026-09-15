using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ABCRetail.Functions
{
    public class ProcessOrder
    {
        private readonly ILogger<ProcessOrder> _logger;

        public ProcessOrder(ILogger<ProcessOrder> logger)
        {
            _logger = logger;
        }

        [Function("ProcessOrder")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestData req)
        {
            _logger.LogInformation( "ProcessOrder function received a request.");

            try
            {
                // Read the order information from the request
                var order = await JsonSerializer.DeserializeAsync<OrderQueueMessage>(req.Body, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (order == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync("Order information is required.");

                    return badRequest;
                }

                // Get Azure Storage connection string
                var connectionString =  Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException( "Azure Storage connection string is not configured.");
                }

                // Get the EXISTING order queue name
                var queueName = Environment.GetEnvironmentVariable("OrderQueueName");

                if (string.IsNullOrWhiteSpace(queueName))
                {
                    throw new InvalidOperationException("Order queue name is not configured.");
                }

                // Connect to the existing Azure Queue
                var queueClient =new QueueClient(connectionString, queueName);

                await queueClient.CreateIfNotExistsAsync();

                // Convert order to JSON
                var message = JsonSerializer.Serialize(order);

                // Write order to Azure Queue
                await queueClient.SendMessageAsync(message);

                _logger.LogInformation("Order {OrderId} added to Azure Queue successfully.",order.OrderId);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Order added to queue successfully.",
                    orderId = order.OrderId,
                    queue = queueName
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error processing order.");

                var response = req.CreateResponse( HttpStatusCode.InternalServerError);

                await response.WriteStringAsync( "An error occurred while processing the order.");

                return response;
            }
        }
    }

    public class OrderQueueMessage
    {
        public string OrderId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Status { get; set; } = "Processing";

        public DateTime CreatedAt { get; set; }
    }
}