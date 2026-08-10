using System.ComponentModel.DataAnnotations;

namespace ABCRetail.ViewModels
{
    public class OrderViewModel
    {
        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
