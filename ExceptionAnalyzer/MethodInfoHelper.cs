namespace ExceptionAnalyzer;

internal static class MethodInfoHelper
{
    public static MethodInfo Flatten(MethodInfo method)
    {
        return new MethodInfo(method.Symbol, method.MethodName, method.ArgumentTypes, Flatten(method.Block));
    }

    private static BlockInfo Flatten(BlockInfo block)
    {
        var flattenedBlock = new BlockInfo();

        var flattenedCatches = block.CatchInfos.Select(Flatten).ToArray();

        // copy over the exceptions from the block itself
        flattenedBlock.ThrownExceptions.AddRange(ExceptionInfoHelper.Combine(block.ThrownExceptions, flattenedCatches));

        // copy over the exceptions from any nested block (method call or block)
        foreach (var nestedBlock in block.Blocks)
        {
            var flattened = Flatten(nestedBlock);

            flattenedBlock.ThrownExceptions.AddRange(ExceptionInfoHelper.Combine(flattened.ThrownExceptions, flattenedCatches));
        }

        return flattenedBlock;
    }

    private static CatchInfo Flatten(CatchInfo @catch)
    {
        return new CatchInfo(@catch.CaughtException, Flatten(@catch.Block));
    }
}
