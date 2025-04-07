namespace Contracts.V1.Responses;

public abstract class ReelElementResponse
{
    public Guid Id { get; set; }
}

public class CheckboxResponse : ReelElementResponse
{
    public required string Header { get; set; }
    public bool IsChecked { get; set; }
}

public class DropdownResponse : ReelElementResponse
{
    public IEnumerable<ReelElementResponse> Elements { get; set; } = [];
}

public class PictureResponse : ReelElementResponse
{
    public required string Url { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public string? Caption { get; set; }
}

public class PlainTextResponse : ReelElementResponse
{
    public required string Text { get; set; }
}