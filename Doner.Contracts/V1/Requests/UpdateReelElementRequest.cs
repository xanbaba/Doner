namespace Contracts.V1.Requests;

public abstract class UpdateReelElementRequest
{
    public required ReelElementType ElementType { get; set; }
}

public class UpdateCheckboxRequest : UpdateReelElementRequest
{
    public required bool IsChecked { get; set; }
    public required string Header { get; set; }
}

public class UpdateDropdownRequest : UpdateReelElementRequest
{
    // TODO: Ensure that default value is not null in endpoint
    public IEnumerable<UpdateReelElementRequest> Elements { get; set; } = [];
}

public class UpdatePlainTextRequest : UpdateReelElementRequest
{
    public required string Text { get; set; }
}

public class UpdatePictureRequest : UpdateReelElementRequest
{
    public required string Url { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
}