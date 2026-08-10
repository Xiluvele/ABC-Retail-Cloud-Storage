namespace ABCRetail.Models
{
    public class OrderQueueMessage
    {
        public string OrderId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Status { get; set; } = "Processing";

        public DateTime CreatedAt { get; set; }
    }
}
