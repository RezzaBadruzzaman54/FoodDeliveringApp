using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class Role
    {
        public Role()
        {
            Buyers = new HashSet<Buyer>();
            Couriers = new HashSet<Courier>();
            Employees = new HashSet<Employee>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Buyer> Buyers { get; set; }
        public virtual ICollection<Courier> Couriers { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
