using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative number.")]
        public int StockQuantity { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp
        {
            get;
            set;
        }
    }
}
