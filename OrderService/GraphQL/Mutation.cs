using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        //Tracking Order
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> AddTrackingOrderAsync(TrackingInput input, [Service] FoodDeliveringAppContext context)
        {
            OrderOutput orderOutput = new();
            orderOutput.TransactionDate = DateTime.Now.ToString();

            var order = context.Orders.FirstOrDefault(o => o.Id == input.OrderId && o.Status != "Cancel");
            if (order == null)
            {
                orderOutput.Message = "Tidak Ada Order!";
                return orderOutput;
            }
            if (order.Status != StatusOrder.OnProses && order.Status != StatusOrder.OnDelivery)
            {
                orderOutput.Message = "Order Tidak Perlu diantar/Sudah Selesai!";
                return orderOutput;
            }

            using var transaction = context.Database.BeginTransaction();
            try
            {
                if (order.Status == StatusOrder.OnProses)
                {
                    OrderTracking orderTracking = new OrderTracking
                    {
                        OrderId = input.OrderId,
                        Location = input.Location
                    };
                    context.OrderTrackings.Add(orderTracking);

                    order.Status = StatusOrder.OnDelivery;
                    context.Orders.Update(order);
                    orderOutput.Message = "Tracking ditambah!";
                }
                else if (order.Status == StatusOrder.OnDelivery)
                {
                    var orderTracking = context.OrderTrackings.FirstOrDefault(o => o.OrderId == order.Id);

                    orderTracking.Location = input.Location;
                    
                    context.OrderTrackings.Update(orderTracking);
                    orderOutput.Message = "Tracking diupdate!";
                }

                context.SaveChanges();
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.Message);
            }
            return orderOutput;
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> CompleteOrderAsync(int id, [Service] FoodDeliveringAppContext context)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id && o.Status != "Cancel");
            var tracking = context.OrderTrackings.Include(ot=> ot.Order).Where(ot=>ot.OrderId == order.Id && ot.Location == order.DestinationAddress).FirstOrDefault();
            if (order == null) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Tidak Ada Order!"
            };
            if (order.Status != StatusOrder.OnDelivery) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order Belum diambil atau Pesanan Sudah Selesai!"
            };

            //var tracking = context.OrderTrackings.Where(ot=>ot.Location == order.DestinationAddress).FirstOrDefault();
            if(tracking==null) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Tidak Dapat Set Completed, pastikan kirim ke lokasi pembeli!"
            };

            order.Status = StatusOrder.Completed;
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order diset Selesai!"
            };
        }


    }
}
