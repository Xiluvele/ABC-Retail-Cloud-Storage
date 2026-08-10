using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class InventoryController : Controller
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly FileStorageService _fileStorageService;

        public InventoryController(QueueStorageService queueStorageService, FileStorageService fileStorageService)
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
        public async Task<IActionResult> Index(
            InventoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var message = new InventoryQueueMessage
            {
                InventoryId =
                    $"INV-{Guid.NewGuid().ToString()[..8].ToUpper()}",

                ProductName = model.ProductName,
                Operation = model.Operation,
                Quantity = model.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            await _queueStorageService
                .SendInventoryMessageAsync(message);

            //log the inventory transaction details to a file in Azure Blob Storage
            var logContent = $"""
                Inventory Transaction Log

                Inventory ID: {message.InventoryId}
                Product: {message.ProductName}
                Operation: {message.Operation}
                Quantity: {message.Quantity}
                Created At: {message.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC
                """;

            var logFileName =
                $"inventory-{message.InventoryId}-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";

            await _fileStorageService
                .UploadLogAsync(
                    logFileName,
                    logContent);

            TempData["SuccessMessage"] =
                $"Inventory transaction {message.InventoryId} sent successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}