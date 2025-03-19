namespace Contracts.V1.Requests;

public class SignInRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}