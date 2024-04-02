namespace Faultr;

public class ThrownExceptionInfo
{
    public ThrownExceptionInfo(Type type, Exception? exception)
    {
        Type = type;
        Exception = exception;
    }

    public Type Type { get; }
    public Exception? Exception { get; }
}
