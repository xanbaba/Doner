namespace Doner.Features.MarkdownFeature.Hubs.Models;

// Request DTOs

public class OperationRequest
{
    public Guid OperationId { get; set; } = Guid.NewGuid();
    public int BaseVersion { get; set; }
    public List<ComponentDto> Components { get; set; } = new();
}

public enum ComponentType
{
    Retain,
    Insert,
    Delete
}

public class ComponentDto
{
    public ComponentType Type { get; set; }
    public int? Count { get; set; }          // For retain and delete
    public string? Text { get; set; }        // For insert
}

public class CursorPositionRequest
{
    public int Position { get; set; }        // Character position in document
    public bool HasSelection { get; set; }
    public int? SelectionStart { get; set; } // Start position of selection (if any)
    public int? SelectionEnd { get; set; }   // End position of selection (if any)
}

// Response DTOs

public class UserInfoResponse
{
    public string UserId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Color { get; set; } = null!;
    public bool IsTyping { get; set; }
}

public class DocumentStateResponse
{
    public string Content { get; set; } = null!;
    public int Version { get; set; }
}

public class OperationResponse
{
    public string OperationId { get; set; } = null!;
    public List<ComponentDto> Components { get; set; } = new();
    public int BaseVersion { get; set; }
    public string UserId { get; set; } = null!;
}

public class CursorPositionResponse
{
    public int Position { get; set; }
    public bool HasSelection { get; set; }
    public int? SelectionStart { get; set; }
    public int? SelectionEnd { get; set; }
}
