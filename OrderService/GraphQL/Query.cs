using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<OrderTracking> GetOrderTrackings([Service] FoodDeliveringAppContext context, ClaimsPrincipal claimsPrincipal)
        {
            List<OrderTracking> orderTrackings = new();
            var username = claimsPrincipal.Identity.Name;

            var user = context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null) return orderTrackings.AsQueryable();

            var tracking = context.OrderTrackings.ToList();
            //var orders = context.Buyers.Include(b => b.Orders).ThenInclude(o => o.OrderTrackings).ToList();
            var orders = context.Orders.Include(o => o.Buyer).Include(o => o.OrderTrackings)
                .Where(o => o.Buyer.UserId == user.Id && o.Id == tracking[0].OrderId).ToList();

            foreach (var order in orders)
            {
                if (order.Status == StatusOrder.OnDelivery)
                {
                    foreach (var orderTracking in order.OrderTrackings)
                    {
                        orderTrackings.Add(orderTracking);
                    }
                }
            }
            return orderTrackings.AsQueryable();
        }
    }
}
