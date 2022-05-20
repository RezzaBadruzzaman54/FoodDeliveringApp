using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> GetOrdersByBuyer([Service] FoodDeliveringAppContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
            var buyer = context.Buyers.Where(b => b.UserId == user.Id).FirstOrDefault();
            if (buyer != null)
            {
                var orders = context.Orders.Where(o => o.BuyerId == buyer.Id);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable(); 
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Order> GetManagerOrders([Service] FoodDeliveringAppContext context) =>
          context.Orders;

    }
}
