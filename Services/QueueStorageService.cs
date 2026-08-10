using Azure.Storage.Queues;
using ABCRetail.Models;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient _orderQueueClient;
        private readonly QueueClient _inventoryQueueClient;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            var orderQueueName =
                configuration["AzureStorage:OrderQueueName"];

            var inventoryQueueName =
                configuration["AzureStorage:InventoryQueueName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            if (string.IsNullOrWhiteSpace(orderQueueName))
            {
                throw new InvalidOperationException(
                    "Order queue name is missing.");
            }

            if (string.IsNullOrWhiteSpace(inventoryQueueName))
            {
                throw new InvalidOperationException(
                    "Inventory queue name is missing.");
            }

            _orderQueueClient =
                new QueueClient(
                    connectionString,
                    orderQueueName);

            _inventoryQueueClient =
                new QueueClient(
                    connectionString,
                    inventoryQueueName);

            _orderQueueClient.CreateIfNotExists();
            _inventoryQueueClient.CreateIfNotExists();
        }

        public async Task SendOrderMessageAsync(
            OrderQueueMessage message)
        {
            var json =
                JsonSerializer.Serialize(message);

            await _orderQueueClient
                .SendMessageAsync(json);
        }

        public async Task SendInventoryMessageAsync(
            InventoryQueueMessage message)
        {
            var json =
                JsonSerializer.Serialize(message);

            await _inventoryQueueClient
                .SendMessageAsync(json);
        }
    }
}