using System.Text.Json.Serialization;

namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Represents information about a connected user in the collaborative editing session
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Display name of the user
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// Email address of the user (optional)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Timestamp when the user connected to the document
    /// </summary>
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Random color assigned to this user for cursor/selection highlighting
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Indicates if the user is actively typing
    /// </summary>
    [JsonIgnore] // Don't serialize this state persistently
    public bool IsTyping { get; set; }
    
    /// <summary>
    /// Creates a user info object with default values
    /// </summary>
    public UserInfo()
    {
        // Generate a random color for user highlighting
        Color = GenerateUserColor();
    }
    
    /// <summary>
    /// Generates a random vibrant color for user cursor/selection
    /// </summary>
    private static string GenerateUserColor()
    {
        // List of vibrant, distinguishable colors
        var colors = new[]
        {
            "#FF5733", // Coral red
            "#33A8FF", // Light blue
            "#33FF57", // Light green
            "#FF33A8", // Pink
            "#A833FF", // Purple
            "#FFB833", // Orange
            "#33FFE0", // Teal
            "#FF5733", // Red
            "#4287f5", // Royal blue
            "#42f54e", // Bright green
            "#f5c242", // Golden
            "#f542e0", // Magenta
            "#f54242", // Bright red
            "#4286f4"  // Sky blue
        };
        
        // Select a random color from the list
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }
}
