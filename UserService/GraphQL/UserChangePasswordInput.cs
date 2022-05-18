namespace UserService.GraphQL
{
    public record UserChangePasswordInput
    (
        int? Id,
        string Password
    );
}
