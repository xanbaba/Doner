namespace Doner.Features.MarkdownFeature;

public class RedisOptions
{
    /// <summary>
    /// The Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = null!;
    
    /// <summary>
    /// The timeout for editing sessions (default: 30 minutes)
    /// </summary>
    public TimeSpan SessionTimeout { get; set; }
    
    /// <summary>
    /// How long to keep operations after a session ends (default: 1 day)
    /// </summary>
    public TimeSpan OperationRetention { get; set; }
}
