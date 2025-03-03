namespace Doner.Features.ReelsFeature.Elements;

public abstract class Element(string type)
{
    public string Type { get; set; } = type;
}