namespace ProductService.GraphQL
{
    public class ProductWithCategoryData
    {
        public int FoodId { get; set; } 
        public string FoodName { get; set; }
        public string FoodCategory { get; set; }
        public int Stock { get; set; }
        public int Price { get; set; }
    }
}
