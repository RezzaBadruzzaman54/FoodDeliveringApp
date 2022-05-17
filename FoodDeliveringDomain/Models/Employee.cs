using System;
using System.Collections.Generic;

namespace FoodDeliveringDomain.Models
{
    public partial class Employee
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string EmployeeNumber { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
