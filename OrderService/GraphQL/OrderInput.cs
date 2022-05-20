namespace OrderService.GraphQL
{
    public record OrderInput
    (
        int? Id,
        string? Code,
        int? BuyerId,
        int ProductId,
        int CourierId,
        int Quantity,
        int? TotalCost,
        string DestinationAddress,
        string? Status
    );
}
