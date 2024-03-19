namespace Faultr.Internal.Models;

internal class CatchInfo
{
    public CatchInfo(ExceptionInfo caughtException, BlockInfo block)
    {
        CaughtException = caughtException ?? throw new ArgumentNullException(nameof(caughtException));
        Block = block ?? throw new ArgumentNullException(nameof(block));
    }

    public ExceptionInfo CaughtException { get; } = null!;

    public BlockInfo Block { get; } = null!;
}
