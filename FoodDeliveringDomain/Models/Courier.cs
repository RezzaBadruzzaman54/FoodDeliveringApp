using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class Courier
    {
        public Courier()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string NumberOfVehicles { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; }
    }
}
