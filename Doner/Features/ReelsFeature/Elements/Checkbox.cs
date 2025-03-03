namespace Doner.Features.ReelsFeature.Elements;

public class Checkbox() : Element(nameof(Checkbox))
{
    public bool IsChecked { get; set; }
    public string Text { get; set; } = string.Empty;
}