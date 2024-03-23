namespace ExceptionAnalyzer.Internal.Models;

internal class BlockInfo
{
    public BlockInfo() : this(new(), new(), new())
    {
    }

    public BlockInfo(IReadOnlyList<ExceptionInfo> thrownExceptions) : this(new(), new(), new())
    {
        ThrownExceptions.AddRange(thrownExceptions);
    }

    public BlockInfo(List<ExceptionInfo> thrownExceptions, List<BlockInfo> blocks, List<CatchInfo> catchInfos)
    {
        ThrownExceptions = thrownExceptions ?? throw new ArgumentNullException(nameof(thrownExceptions));
        MethodCalls = new();
        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        CatchInfos = catchInfos ?? throw new ArgumentNullException(nameof(catchInfos));
    }

    public List<ExceptionInfo> ThrownExceptions { get; }

    public List<CallInfo> MethodCalls { get; }

    public List<BlockInfo> Blocks { get; }

    public List<CatchInfo> CatchInfos { get; }
}
