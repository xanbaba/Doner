namespace Contracts.V1.Requests;

public abstract class AddReelElementRequest
{
    public ReelElementType Type { get; set; }
}

public class AddCheckboxRequest : AddReelElementRequest
{
    public required bool IsChecked { get; set; }
    public string? Header { get; set; }
}

public class AddDropdownRequest : AddReelElementRequest
{
    // TODO: Ensure that default value is not null in endpoint
    public IEnumerable<AddReelElementRequest> Elements { get; set; } = [];
}

public class AddPlainTextRequest : AddReelElementRequest
{
    public required string Text { get; set; }
}

public class AddPictureRequest : AddReelElementRequest
{
    public required string Url { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
}