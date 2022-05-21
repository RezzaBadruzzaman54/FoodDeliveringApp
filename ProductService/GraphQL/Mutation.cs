//using FoodDeliveringDomain.Models;
using FoodDeliveringAppDomain.Models;
using HotChocolate.AspNetCore.Authorization;

namespace ProductService.GraphQL
{
    public class Mutation
    {
        //manage Category
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Category> AddCategoryAsync(
          CategoryInput input,
          [Service] FoodDeliveringAppContext context)
        {

            // EF
            var category = new Category
            {
                Name = input.Name,
                Description = input.Description
            };

            var ret = context.Categories.Add(category);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Category> UpdateCategoryAsync(
          CategoryInput input,
          [Service] FoodDeliveringAppContext context)
        {
            var category = context.Categories.Where(c => c.Id == input.Id).FirstOrDefault();
            if (category != null)
            {
                category.Name = input.Name;
                category.Description = input.Description;


                context.Categories.Update(category);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(category);
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Category> DeleteCategoryByIdAsync(
           int id,
           [Service] FoodDeliveringAppContext context)
        {
            var category = context.Categories.Where(c => c.Id == id).FirstOrDefault();
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(category);
        }

        //manage Food
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Product> AddFoodAsync(
           ProductInput input,
           [Service] FoodDeliveringAppContext context)
        {

            // EF
            var product = new Product
            {
                Name = input.Name,
                CategoryId = input.CategoryId,
                Stock = input.Stock,
                Price = input.Price,
            };

            var productCategory = context.Categories.Where(c => c.Id == input.CategoryId).FirstOrDefault();
            if (productCategory == null) return new Product();

            var ret = context.Products.Add(product);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Product> UpdateFoodAsync(
          ProductInput input,
          [Service] FoodDeliveringAppContext context)
        {
            var product = context.Products.Where(o => o.Id == input.Id).FirstOrDefault();
            if (product != null)
            {
                product.Name = input.Name;
                product.CategoryId = input.CategoryId;
                product.Stock = input.Stock;
                product.Price = input.Price;

                context.Products.Update(product);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(product);
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Product> DeleteFoodByIdAsync(
          int id,
          [Service] FoodDeliveringAppContext context)
        {
            var product = context.Products.Where(o => o.Id == id).FirstOrDefault();
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(product);
        }
    }
}
