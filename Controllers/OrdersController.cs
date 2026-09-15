using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly AzureFunctionService _azureFunctionService;

        public OrdersController(
            FileStorageService fileStorageService,
            AzureFunctionService azureFunctionService)
        {
            _fileStorageService = fileStorageService;
            _azureFunctionService = azureFunctionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            OrderViewModel model)
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

            // Send the order to the Azure Function.
            // The Azure Function will then send it
            // to the existing order-processing queue.
            var orderSent =
                await _azureFunctionService
                    .SendOrderAsync(message);

            if (!orderSent)
            {
                TempData["ErrorMessage"] =
                    "The order could not be sent for processing.";

                return View(model);
            }

            // Create an order processing log.
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

            // Upload the order log to Azure Files.
            var fileUploaded = await _azureFunctionService.UploadFileAsync(logFileName,logContent);

            if (!fileUploaded)
            {
                TempData["ErrorMessage"] =
                    "The order was sent to the queue, but the log file could not be uploaded.";
            }

            TempData["SuccessMessage"] =
                $"Order {message.OrderId} sent to Azure Queue successfully.";

            ModelState.Clear();

            return RedirectToAction(nameof(Index));
        }
    }
}

   