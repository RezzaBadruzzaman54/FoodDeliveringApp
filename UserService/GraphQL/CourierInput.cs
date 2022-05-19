namespace UserService.GraphQL
{
    public record CourierInput
   (
        int? Id,
        int UserId,
        int RoleId,
        string NumberOfVehicles,
        string FullName,
        string Email,
        string UserName,
        string Password
    );
}
