namespace ABCRetail.Models
{
    public class InventoryQueueMessage
    {
        public string InventoryId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Operation { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
