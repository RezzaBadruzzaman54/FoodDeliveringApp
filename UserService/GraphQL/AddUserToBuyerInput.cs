namespace UserService.GraphQL
{
    public record AddUserToBuyerInput
    (
        int? Id,
        int UserId,
        int RoleId,
        string Address
    );
}
