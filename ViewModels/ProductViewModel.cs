using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ABCRetail.ViewModels
{
    public class ProductViewModel
    {
        public string? RowKey { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? Image { get; set; }

        [IgnoreDataMember]
        public string? DisplayImageUrl { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}