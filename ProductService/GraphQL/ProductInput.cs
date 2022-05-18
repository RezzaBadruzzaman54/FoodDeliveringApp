namespace ProductService.GraphQL
{
    public record ProductInput
    (
        int? Id,
        string Name,
        int CategoryId,
        int Stock,
        int Price
    );
}
