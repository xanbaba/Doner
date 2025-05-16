namespace Contracts.V1.Requests;

public class SignUpRequest
{
    public required string Username { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
}