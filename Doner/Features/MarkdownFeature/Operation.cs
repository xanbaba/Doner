namespace Doner.Features.MarkdownFeature;

public class Operation
{
    public Guid Id { get; set; }
    public string MarkdownId { get; set; } = null!;
    public Guid UserId { get; set; }
    public int BaseVersion { get; set; } // The version this operation is based on
    public IEnumerable<OperationComponent> Components { get; set; } = [];
    public DateTime Timestamp { get; set; }
}

public abstract class OperationComponent;

public class RetainComponent : OperationComponent
{
    public int Count { get; set; }
}

public class InsertComponent : OperationComponent
{
    public required string Text { get; set; }
}

public class DeleteComponent : OperationComponent
{
    public int Count { get; set; }
}