namespace UserService.GraphQL
{
    public record AddUserToCourierInput
    (
        int? Id,
        int UserId,
        int RoleId,
        string NumberOfVehicles
    );
}
