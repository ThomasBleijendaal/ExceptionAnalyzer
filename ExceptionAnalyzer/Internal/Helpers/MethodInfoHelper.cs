using ExceptionAnalyzer.Internal.Models;

namespace ExceptionAnalyzer.Internal.Helpers;

internal static class MethodInfoHelper
{
    public static MethodInfo Flatten(MethodInfo method)
    {
        var exceptions = ResolveExceptions(method.Block);

        return new MethodInfo(method.Symbol, method.MethodName, method.ArgumentTypes, new BlockInfo(exceptions));
    }

    private static IReadOnlyList<ExceptionInfo> ResolveExceptions(BlockInfo block)
    {
        var exceptions = new List<ExceptionInfo>(block.ThrownExceptions);

        foreach (var nestedBlock in block.Blocks)
        {
            exceptions.AddRange(ResolveExceptions(nestedBlock));
        }

        return ResolveExceptions(exceptions, block.CatchInfos);
    }

    private static IReadOnlyList<ExceptionInfo> ResolveExceptions(
        IReadOnlyList<ExceptionInfo> exceptions,
        IReadOnlyList<CatchInfo> catches)
    {
        if (catches.Count == 0)
        {
            return exceptions;
        }

        var flattenedCatches = catches.Select(@catch =>
        {
            var exceptions = ResolveExceptions(@catch.Block);
            return new CatchInfo(@catch.CaughtException, new BlockInfo(exceptions));
        }).ToArray();

        return ExceptionInfoHelper.Combine(exceptions, flattenedCatches).ToArray();
    }
}
