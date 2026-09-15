using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly AzureFunctionService _azureFunctionService;
        public CustomersController(TableStorageService tableStorageService, AzureFunctionService azureFunctionService)
        {
            _tableStorageService = tableStorageService;
            _azureFunctionService = azureFunctionService;
        }
        public async Task<IActionResult> Index( string search,int page = 1)
        {
            int pageSize = 10;

            var customers =
                await _tableStorageService.GetCustomersAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                customers = customers
                    .Where(c =>
                        (c.FirstName ?? "").ToLower().Contains(search) ||
                        (c.LastName ?? "").ToLower().Contains(search) ||
                        (c.Email ?? "").ToLower().Contains(search) ||
                        (c.PhoneNumber ?? "").ToLower().Contains(search) ||
                        (c.Address ?? "").ToLower().Contains(search))
                    .ToList();
            }

            var totalCustomers = customers.Count();

            var pagedCustomers = customers
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalCustomers / (double)pageSize);

            ViewBag.Search = search;

            return View(pagedCustomers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            //await _tableStorageService.AddCustomerAsync(customer);
            var success = await _azureFunctionService.StoreCustomerAsync(customer);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create customer. Please try again.");

                return View(customer);
            }

            TempData["SuccessMessage"] = "Customer created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var customer = await _tableStorageService.GetCustomerAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var customer = await _tableStorageService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer)
        {
            if (id != customer.RowKey)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(customer);
            }
            await _tableStorageService.UpdateCustomerAsync(customer);
            TempData["SuccessMessage"] = "Customer updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var customer = await _tableStorageService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tableStorageService.DeleteCustomerAsync(id);
            TempData["SuccessMessage"] = "Customer deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
