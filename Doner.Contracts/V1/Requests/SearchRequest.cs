namespace Contracts.V1.Requests;

public class SearchRequest
{
    public string? Query { get; set; }
    public SearchOption? SearchOption { get; set; }
}