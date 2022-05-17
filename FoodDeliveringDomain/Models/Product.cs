using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class Product
    {
        public Product()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public int Stock { get; set; }
        public int Price { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; }
    }
}
