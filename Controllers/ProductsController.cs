using ABCRetail.Models;
using ABCRetail.Services;
using ABCRetail.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;

        public ProductsController(TableStorageService tableStorageService,BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products =
                await _tableStorageService.GetProductsAsync();

            foreach (var product in products)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    product.ImageUrl =
                        _blobStorageService.GetImageUrl(product.ImageUrl);
                }
            }

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? imageUrl = null;

            if (model.Image != null)
            {
                imageUrl =
                    await _blobStorageService.UploadImageAsync(model.Image);
            }

            var product = new Product
            {
                Name = model.ProductName,
                Description = model.Description,
                Category = model.Category,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                ImageUrl = imageUrl
            };

            await _tableStorageService.AddProductAsync(product);

            TempData["SuccessMessage"] =
                "Product created successfully!";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var product =
                await _tableStorageService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                product.ImageUrl =
                    _blobStorageService.GetImageUrl(product.ImageUrl);
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var product =
                await _tableStorageService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var displayImageUrl =
                string.IsNullOrWhiteSpace(product.ImageUrl)
                    ? null
                    : _blobStorageService.GetImageUrl(product.ImageUrl);

            var model = new ProductViewModel
            {
                RowKey = product.RowKey,
                ProductName = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                StockQuantity = product.StockQuantity,

                // Use the SAS URL for displaying the image
                ExistingImageUrl = displayImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,ProductViewModel model)
        {
            if (id != model.RowKey)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingProduct = await _tableStorageService.GetProductAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            var imageUrl = existingProduct.ImageUrl;

            if (model.Image != null)
            {
                await _blobStorageService.DeleteImageAsync(existingProduct.ImageUrl);

                imageUrl =
                    await _blobStorageService
                        .UploadImageAsync(model.Image);
            }

            existingProduct.Name = model.ProductName;
            existingProduct.Description = model.Description;
            existingProduct.Category = model.Category;
            existingProduct.Price = model.Price;
            existingProduct.StockQuantity = model.StockQuantity;
            existingProduct.ImageUrl = imageUrl;

            await _tableStorageService.UpdateProductAsync(existingProduct);

            TempData["SuccessMessage"] =
                "Product updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var product =
                await _tableStorageService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                product.ImageUrl =
                    _blobStorageService.GetImageUrl(product.ImageUrl);
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var product =
                await _tableStorageService.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            await _blobStorageService
                .DeleteImageAsync(product.ImageUrl);

            await _tableStorageService.DeleteProductAsync(id);

            TempData["SuccessMessage"] =
                "Product deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}