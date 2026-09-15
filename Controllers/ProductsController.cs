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
        private readonly AzureFunctionService _azureFunctionService;

        public ProductsController(TableStorageService tableStorageService,BlobStorageService blobStorageService, AzureFunctionService azureFunctionService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _azureFunctionService = azureFunctionService;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 8;

            var products =
                await _tableStorageService.GetProductsAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                products = products
                    .Where(p =>
                        (p.Name ?? "").ToLower().Contains(search) ||
                        (p.Category ?? "").ToLower().Contains(search) ||
                        (p.Description ?? "").ToLower().Contains(search))
                    .ToList();
            }

            var totalProducts = products.Count();

            var pagedProducts = products
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var product in pagedProducts)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    product.ImageUrl =
                        _blobStorageService.GetImageUrl(product.ImageUrl);
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewBag.Search = search;

            return View(pagedProducts);
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
                imageUrl = await _azureFunctionService.UploadProductImageAsync(model.Image);

                if (imageUrl == null)
                {
                    ModelState.AddModelError( "", "Unable to upload the product image. Please try again.");

                    return View(model);
                }
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

                imageUrl = await _azureFunctionService.UploadProductImageAsync(model.Image);

                if (imageUrl == null)
                {
                    ModelState.AddModelError( "", "Unable to upload the new product image. Please try again.");

                    return View(model);
                }
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