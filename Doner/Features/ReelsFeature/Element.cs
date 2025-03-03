namespace Doner.Features.ReelsFeature;

public abstract class Element(string type)
{
    public string Type { get; set; } = type;
}