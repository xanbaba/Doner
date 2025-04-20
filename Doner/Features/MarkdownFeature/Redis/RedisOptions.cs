namespace Doner.Features.MarkdownFeature.Redis;

public class RedisOptions
{
    /// <summary>
    /// The Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>
    /// The timeout for editing sessions (default: 30 minutes)
    /// </summary>
    public TimeSpan? SessionTimeout { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// How long to keep operations after a session ends (default: 1 day)
    /// </summary>
    public TimeSpan? OperationRetention { get; set; } = TimeSpan.FromDays(1);
}
