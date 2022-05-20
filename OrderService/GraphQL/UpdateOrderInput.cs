namespace OrderService.GraphQL
{
    public record UpdateOrderInput
    (
        int? Id,
        int CourierId
    );
}
