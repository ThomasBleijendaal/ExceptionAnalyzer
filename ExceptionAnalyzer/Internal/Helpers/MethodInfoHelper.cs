using ExceptionAnalyzer.Internal.Models;

namespace ExceptionAnalyzer.Internal.Helpers;

internal static class MethodInfoHelper
{
    public static MethodInfo Flatten(MethodInfo method)
    {
        var exceptions = ResolveExceptions(method.Block);

        var flattenedBlock = new BlockInfo(exceptions);
        //flattenedBlock.ThrownExceptions.AddRange(exceptions);

        return new MethodInfo(method.Symbol, method.MethodName, method.ArgumentTypes, flattenedBlock); // FlattenOld(method.Block));
    }

    // TODO: this must start at deepest level first and work back
    private static BlockInfo FlattenOld(BlockInfo block)
    {
        var flattenedBlock = new BlockInfo();

        var flattenedCatches = block.CatchInfos.Select(Flatten).ToArray();

        // copy over the exceptions from the block itself
        flattenedBlock.ThrownExceptions.AddRange(ExceptionInfoHelper.Combine(block.ThrownExceptions, flattenedCatches));

        // copy over the exceptions from any nested block (method call or block)
        foreach (var nestedBlock in block.Blocks)
        {
            var flattened = FlattenOld(nestedBlock);

            flattenedBlock.ThrownExceptions.AddRange(ExceptionInfoHelper.Combine(flattened.ThrownExceptions, flattenedCatches));
        }

        return flattenedBlock;
    }

    private static CatchInfo Flatten(CatchInfo @catch)
    {
        return new CatchInfo(@catch.CaughtException, FlattenOld(@catch.Block));
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
