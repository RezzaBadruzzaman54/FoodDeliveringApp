using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderTrackings = new HashSet<OrderTracking>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int BuyerId { get; set; }
        public int ProductId { get; set; }
        public int CourierId { get; set; }
        public int Quantity { get; set; }
        public int TotalCost { get; set; }
        public string DestinationAddress { get; set; } = null!;
        public string Status { get; set; } = null!;

        public virtual Buyer Buyer { get; set; } = null!;
        public virtual Courier Courier { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; }
    }
}
