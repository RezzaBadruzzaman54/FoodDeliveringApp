using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderOutput> BuyFoodsByBuyerAsync(
           List<OrderInput> input,
           ClaimsPrincipal claimsPrincipal,
           [Service] FoodDeliveringAppContext context)
        {
            using var transaction = context.Database.BeginTransaction();

            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
                var buyer = context.Buyers.Where(b=>b.UserId == user.Id).FirstOrDefault();
                var courier = context.Couriers.Where(c => c.Id == input[0].CourierId).FirstOrDefault();
                if (courier == null) return new OrderOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Courier Not Available!"
                };

                if (user != null)
                {
                    var code = Guid.NewGuid().ToString();
                    List<Order> dataOrders = new();
                    foreach (var item in input)
                    {                    
                        var product = context.Products.Where(p=>p.Id == item.ProductId).FirstOrDefault();
                        var order = new Order
                        {
                            Code = code,
                            BuyerId = buyer.Id,
                            CourierId = item.CourierId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            TotalCost = product.Price * item.Quantity,
                            DestinationAddress = item.DestinationAddress,
                            Status = StatusOrder.Waiting
                        };
                        dataOrders.Add(order);
                    }
                    context.Orders.AddRange(dataOrders);
                    context.SaveChanges();
                    await transaction.CommitAsync();

                    return new OrderOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Berhasil Membuat Order!"
                    };
                }
                else
                    throw new Exception("user was not found");
            }
            catch (Exception err)
            {
                transaction.Rollback();

                return new OrderOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = err.Message.ToString()
                };
            }
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<OrderOutput> ConfirmOrderAsync(int id, [Service] FoodDeliveringAppContext context)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id && o.Status != "Cancel");
            if (order == null) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Tidak Ada Order!"
            };

            if (order.Status != StatusOrder.Waiting) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Tidak Perlu Konfirmasi Order: Pesanan Sudah Diantar atau Proses Sudah Selesai!"
            };

            order.Status = StatusOrder.OnProses;
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Berhasil Confirm Order!"
            };
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> UpdateOrderAsync(
        UpdateOrderInput input,
         [Service] FoodDeliveringAppContext context)
        {
            var order = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            if (order != null)
            {
                order.CourierId = input.CourierId;

                context.Orders.Update(order);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(order);
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> DeleteOrderByIdAsync(
           int id,
           [Service] FoodDeliveringAppContext context)
        {
            var order = context.Orders.Where(c => c.Id == id).FirstOrDefault();
            if (order != null)
            {
                order.Status = "Cancel";
                context.Orders.Update(order);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Tidak Ada Data Order untuk Id {id}.");
            }
            return await Task.FromResult(order);
        }

        //Tracking

    }
}
