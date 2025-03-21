using Doner.DataBase;
using Doner.Features.AuthFeature.Entities;
using Doner.Features.AuthFeature.Exceptions;
using Doner.Features.AuthFeature.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Doner.Tests;

public class RefreshTokensManagerTests
{
    private readonly AppDbContext _dbContext;
    private readonly RefreshTokensManager _refreshTokensManager;

    public RefreshTokensManagerTests()
    {
        var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection([
            new KeyValuePair<string, string?>("RefreshTokenLifetimeDays", "7")
        ]);
        
        var configuration = configurationBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _dbContext = new AppDbContext(options);

        _refreshTokensManager = new RefreshTokensManager(_dbContext, configuration);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewToken_WhenTokenIsValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "valid-token",
            UtcExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _refreshTokensManager.RefreshTokenAsync("valid-token");

        // Assert
        _ = result.Match(token => token.Should().NotBe("valid-token"),
            _ => throw new Exception("This should not happen"));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenIsNotFound()
    {
        // Act
        var result = await _refreshTokensManager.RefreshTokenAsync("invalid-token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IfFail(e => e.Should().BeOfType<RefreshTokenNotFoundException>());
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenIsExpired()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "expired-token",
            UtcExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _refreshTokensManager.RefreshTokenAsync("expired-token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IfFail(e => e.Should().BeOfType<RefreshTokenExpiredException>());
    }

    [Fact]
    public async Task AssignRefreshTokenAsync_ShouldAssignNewTokenToUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };

        // Act
        var result = await _refreshTokensManager.AssignRefreshTokenAsync(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IfSucc(token =>
        {
            var refreshToken = _dbContext.RefreshTokens.FirstOrDefault(t => t.UserId == user.Id);
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().Be(token);
        });
    }
}