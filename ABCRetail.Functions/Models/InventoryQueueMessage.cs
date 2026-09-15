using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail.Functions.Models
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
