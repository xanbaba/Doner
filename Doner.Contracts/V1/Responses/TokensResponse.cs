namespace Contracts.V1.Responses;

public class TokensResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}