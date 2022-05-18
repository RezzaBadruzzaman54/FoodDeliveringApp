namespace UserService.GraphQL
{
    public record AddUserToEmployeeInput
    (
        int? Id,
        int UserId,
        int RoleId,
        string EmployeeNumber
    );
}
