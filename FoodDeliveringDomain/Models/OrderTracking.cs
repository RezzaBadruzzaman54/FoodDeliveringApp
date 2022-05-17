using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class OrderTracking
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Location { get; set; } = null!;

        public virtual Order Order { get; set; } = null!;
    }
}
