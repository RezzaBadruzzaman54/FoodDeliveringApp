using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ProductService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "BUYER", "MANAGER"})]
        public IQueryable<Product> GetAllFoods([Service] FoodDeliveringAppContext context) =>
            context.Products;

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<ProductWithCategoryData> GetFoodByCategoryName([Service] FoodDeliveringAppContext context, string categoryName)
        {
            var productByCategory = context.Products.Include(p => p.Category).Where(c => c.Name == categoryName).FirstOrDefault();
            if(productByCategory != null)
            {
                return context.Products.Include(p => p.Category).Where(c => c.Name == categoryName).Select(p => new ProductWithCategoryData()
                {
                    FoodId = p.Id,
                    FoodName = p.Name,
                    FoodCategory = p.Category.Name,
                    Stock = p.Stock,
                    Price = p.Price
                });
            }
            return new List<ProductWithCategoryData>().AsQueryable();
        }
    }
}
