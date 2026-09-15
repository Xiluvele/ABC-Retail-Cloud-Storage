using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ABCRetail.Controllers
{
    public class InventoryController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly AzureFunctionService _azureFunctionService;
        private readonly TableStorageService _tableStorageService;

        public InventoryController(
            FileStorageService fileStorageService,
            AzureFunctionService azureFunctionService,
            TableStorageService tableStorageService)
        {
            _fileStorageService = fileStorageService;
            _azureFunctionService = azureFunctionService;
            _tableStorageService = tableStorageService;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            var model = new InventoryViewModel();

            await LoadProductsAsync(model);

            return View(model);
        }

        // POST: Inventory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            InventoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the products if validation fails.
                await LoadProductsAsync(model);

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

            // Send the inventory transaction
            // to the ProcessInventory Azure Function.
            var inventorySent =
                await _azureFunctionService
                    .SendInventoryAsync(message);

            if (!inventorySent)
            {
                TempData["ErrorMessage"] =
                    "The inventory transaction could not be sent.";

                await LoadProductsAsync(model);

                return View(model);
            }

            // Create an inventory transaction log.
            // This is currently stored in Azure Files.
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

            var fileUploaded = await _azureFunctionService .UploadFileAsync(logFileName,logContent);

            if (!fileUploaded)
            {
                TempData["ErrorMessage"] ="The inventory transaction was sent, but the log file could not be uploaded.";
            }

            TempData["SuccessMessage"] = $"Inventory transaction {message.InventoryId} sent successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Loads products from Azure Table Storage
        // and displays them in the Product dropdown.
        private async Task LoadProductsAsync(InventoryViewModel model)
        {
            var products = await _tableStorageService.GetProductsAsync();

            model.Products = products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Name,
                    Text = p.Name
                })
                .ToList();
        }
    }
}