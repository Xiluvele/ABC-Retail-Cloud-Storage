using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly FileStorageService _fileStorageService;

        public OrdersController(QueueStorageService queueStorageService, FileStorageService fileStorageService)
        {
            _queueStorageService = queueStorageService;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var message = new OrderQueueMessage
            {
                OrderId =
                    $"ORD-{Guid.NewGuid().ToString()[..8].ToUpper()}",

                CustomerName = model.CustomerName,
                ProductName = model.ProductName,
                Quantity = model.Quantity,
                Status = "Processing",
                CreatedAt = DateTime.UtcNow
            };

            await _queueStorageService
                .SendOrderMessageAsync(message);

            //log the order details to a file in Azure Blob Storage
            var logContent = $"""
                Order Processing Log

                Order ID: {message.OrderId}
                Customer: {message.CustomerName}
                Product: {message.ProductName}
                Quantity: {message.Quantity}
                Status: {message.Status}
                Created At: {message.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC
                """;

            var logFileName =
                $"order-{message.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

            await _fileStorageService
                .UploadLogAsync(
                    logFileName,
                    logContent);

            TempData["SuccessMessage"] =
                $"Order {message.OrderId} sent to Azure Queue successfully.";

            ModelState.Clear();

            return RedirectToAction(nameof(Index));
        }
    }
}