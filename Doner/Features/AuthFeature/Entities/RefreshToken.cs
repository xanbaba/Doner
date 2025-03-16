namespace Doner.Features.AuthFeature.Entities;

public class RefreshToken
{
    public int Id { get; init; }
    public Guid UserId { get; init; }
    public User User { get; init; } = null!;
    public string Token { get; set; } = null!;
    public DateTime UtcExpiresAt { get; set; }
}