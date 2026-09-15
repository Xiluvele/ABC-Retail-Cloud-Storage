using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ABCRetail.ViewModels
{
    public class InventoryViewModel
    {
        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string Operation { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // Products displayed in the dropdown
        public List<SelectListItem> Products { get; set; } = new();
    }
}
