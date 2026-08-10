using Azure;
using Azure.Data.Tables;
using ABCRetail.Models;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];

            var customerTableName = configuration["AzureStorage:CustomerTableName"];

            //product table name
            var productTableName = configuration["AzureStorage:ProductTableName"];


            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Azure Storage connection string or table name is not configured.");
            }

            if (string.IsNullOrWhiteSpace(customerTableName))
            {
                throw new InvalidOperationException("Customer table name is not configured.");
            }
            if (string.IsNullOrWhiteSpace(productTableName))
            {
                throw new InvalidOperationException("Product table name is not configured.");
            }

            _customerTableClient = new TableClient(connectionString, customerTableName);
            _productTableClient = new TableClient(connectionString, productTableName);

            _customerTableClient.CreateIfNotExists();
            _productTableClient.CreateIfNotExists();
        }
        
        public async Task <List<Customer>> GetCustomersAsync()
        {
            var customers = new List<Customer>();

            await foreach(var customer in _customerTableClient.QueryAsync<Customer>(customer => customer.PartitionKey == "Customer"))
            {
                customers.Add(customer);
            }

            return customers;
        }

        public async Task<Customer?> GetCustomerAsync(string rowKey)
        {
            try
            {
                var response = await _customerTableClient.GetEntityAsync<Customer>("Customer", rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            customer.PartitionKey = "Customer";
            customer.RowKey = Guid.NewGuid().ToString();
            await _customerTableClient.AddEntityAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            customer.PartitionKey = "Customer";
            await _customerTableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync("Customer", rowKey);
        }

        // Product methods
        public async Task<List<Product>> GetProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in
                _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }

        public async Task<Product?> GetProductAsync(string rowKey)
        {
            try
            {
                var response =
                    await _productTableClient.GetEntityAsync<Product>(
                        "Product",
                        rowKey);

                return response.Value;
            }
            catch (RequestFailedException ex)
                when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();

            await _productTableClient.AddEntityAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            product.PartitionKey = "Product";

            await _productTableClient.UpdateEntityAsync(
                product,
                ETag.All,
                TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(
                "Product",
                rowKey);
        }

    }
}
