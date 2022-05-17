using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class User
    {
        public User()
        {
            Buyers = new HashSet<Buyer>();
            Couriers = new HashSet<Courier>();
            Employees = new HashSet<Employee>();
        }

        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

        public virtual ICollection<Buyer> Buyers { get; set; }
        public virtual ICollection<Courier> Couriers { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
