namespace Doner.Features.MarkdownFeature;

public class OperationComponent
{
    public OperationComponentType Type { get; set; }
    public string Text { get; set; } = string.Empty; // For Insert operations
    public int Count { get; set; } // For Retain and Delete operations

    public static OperationComponent Insert(string text)
    {
        return new OperationComponent { Type = OperationComponentType.Insert, Text = text };
    }

    public static OperationComponent Delete(int count)
    {
        return new OperationComponent { Type = OperationComponentType.Delete, Count = count };
    }

    public static OperationComponent Retain(int count)
    {
        return new OperationComponent { Type = OperationComponentType.Retain, Count = count };
    }
}