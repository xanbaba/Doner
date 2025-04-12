namespace Doner.Features.MarkdownFeature.Exceptions;

public class MarkdownNotFoundException : Exception
{
    public MarkdownNotFoundException() : base("Markdown not found.") { }
}