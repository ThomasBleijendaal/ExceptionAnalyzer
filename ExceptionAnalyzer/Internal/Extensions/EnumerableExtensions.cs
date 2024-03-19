using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Extensions;

internal static class EnumerableExtensions
{
    public static bool AreEqual<T>(this IReadOnlyList<T> source, IReadOnlyList<T> target)
        where T : ISymbol?
    {
        if (source.Count != target.Count)
        {
            return false;
        }

        return source
            .Select((item, index) => (source: item, target: target[index]))
            .All(items => items switch
            {
                (null, null) => true,
                (null, { }) => false,
                ({ }, null) => false,
                ({ } a, { } b) => SymbolEqualityComparer.IncludeNullability.Equals(a, b)
            });
    }
}
