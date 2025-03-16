namespace Doner.Features.ReelsFeature.Services;

public enum SearchOption
{
    /// <summary>
    /// indicates that search query should fully match.
    /// </summary>
    FullMatch,
    
    /// <summary>
    /// indicates that search query should be partially matched. Matches should at least contain the query.
    /// </summary>
    PartialMatch,
}