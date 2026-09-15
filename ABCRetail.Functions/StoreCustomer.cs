using ABCRetail.Functions.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ABCRetail.Functions
{
    public class StoreCustomer
    {
        private readonly ILogger<StoreCustomer> _logger;

        public StoreCustomer(ILogger<StoreCustomer> logger)
        {
            _logger = logger;
        }

        [Function("StoreCustomer")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestData req)
        {
            _logger.LogInformation(
                "StoreCustomer function received a request.");

            try
            {
                //var customer = await JsonSerializer.DeserializeAsync<Customer>(req.Body);
                var customer =
                await JsonSerializer.DeserializeAsync<Customer>( req.Body, new JsonSerializerOptions
                     {
                           PropertyNameCaseInsensitive = true
                     });

                if (customer == null)
                {
                    var badRequest =req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync("Customer information is required.");

                    return badRequest;
                }

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException( "Azure Storage connection string is not configured.");
                }

                var tableClient = new TableClient(connectionString,"Customer");

                await tableClient.CreateIfNotExistsAsync();

                customer.PartitionKey = "Customer";

                if (string.IsNullOrWhiteSpace(customer.RowKey))
                {
                    customer.RowKey = Guid.NewGuid().ToString();
                }

                await tableClient.AddEntityAsync(customer);

                _logger.LogInformation( "Customer {CustomerId} stored successfully.", customer.RowKey);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Customer stored successfully.",
                    customerId = customer.RowKey,
                    table = "Customer"
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(  ex,"Error storing customer in Azure Table Storage.");

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteStringAsync("An error occurred while storing the customer.");

                return response;
            }
        }
    }
}