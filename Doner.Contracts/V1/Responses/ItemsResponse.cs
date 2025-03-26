namespace Contracts.V1.Responses;

public class ItemsResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
}