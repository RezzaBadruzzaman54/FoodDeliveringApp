using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;

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
    }
}
