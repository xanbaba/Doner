namespace Doner.Tests;

public static class EnumerableExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> source, T value, EqualityComparer<T>? comparer = null)
    {
        var index = 0;
        comparer ??= EqualityComparer<T>.Default;
        foreach (var item in source)
        {
            if (comparer.Equals(item, value)) return index;
            index++;
        }
        return -1;
    }
    
    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var index = 0;
        foreach (var item in source)
        {
            if (predicate(item)) return index;
            index++;
        }
        return -1;
    }
}