using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveringAppContext context) =>
         context.Users.Select(p => new UserData()
         {
             Id = p.Id,
             FullName = p.FullName,
             Email = p.Email,
             UserName = p.UserName
         });

        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<Buyer> GetBuyers([Service] FoodDeliveringAppContext context) =>
        context.Buyers;

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<CourierData> GetCouriers([Service] FoodDeliveringAppContext context) =>
         context.Couriers.Include(c=>c.User).Select(p => new CourierData()
         {
             Id = p.Id,
             UserName = p.User.UserName,
             FullName = p.User.FullName,
             Email = p.User.Email,
             NumberOfVehicles = p.NumberOfVehicles
         });

        [Authorize] // dapat diakses kalau sudah login
        public IQueryable<ProfilUserData> GetProfileUser(
            [Service] FoodDeliveringAppContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check role ?
            var userRole = claimsPrincipal.Claims.Where(
                claim => claim.Type == ClaimTypes.Role).FirstOrDefault();

            var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                
                if (userRole.Value.Equals("BUYER"))
                {
                   return context.Buyers.Include(b => b.User).Include(b => b.Role).Where(u => u.Id == user.Id).Select(p => new ProfilUserData()
                    {

                        UserId = p.Id,
                        FullName = p.User.FullName,
                        Email = p.User.Email,
                        RoleId = p.RoleId,
                        RoleName = p.Role.Name,
                        UserName = p.User.UserName,
                        Password = p.User.Password
                    });

                }
                else if (userRole.Value.Equals("COURIER"))
                {
                   return context.Couriers.Include(c => c.User).Include(c => c.Role).Where(u => u.Id == user.Id).Select(p => new ProfilUserData()
                    {
                        UserId = p.Id,
                        FullName = p.User.FullName,
                        Email = p.User.Email,
                        RoleId = p.RoleId,
                        RoleName = p.Role.Name,
                        UserName = p.User.UserName,
                        Password = p.User.Password
                    });
                }
                else if(userRole.Value.Equals("ADMIN") || userRole.Value.Equals("MANAGER"))
                {
                   return context.Employees.Include(e => e.User).Include(e => e.Role).Where(u => u.Id == user.Id).Select(p => new ProfilUserData()
                    {
                        UserId = p.Id,
                        FullName = p.User.FullName,
                        Email = p.User.Email,
                        RoleId = p.RoleId,
                        RoleName = p.Role.Name,
                        UserName = p.User.UserName,
                        Password = p.User.Password
                    });
                }
                else
                {
                   Console.WriteLine("Role Tidak Terdaftar");
                }
            }
            return new List<ProfilUserData>().AsQueryable();
        }

    }
}
