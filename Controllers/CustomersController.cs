using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomersController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }
        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetCustomersAsync();

            return View(customers);
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
            
            await _tableStorageService.AddCustomerAsync(customer);

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
