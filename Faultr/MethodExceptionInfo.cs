namespace Faultr;

public sealed class MethodExceptionInfo
{
    public MethodExceptionInfo(Type type, string methodName, ThrownExceptionInfo[] exceptions)
    {
        Type = type;
        MethodName = methodName;
        ThrownExceptions = exceptions;
    }

    public Type Type { get; }
    public string MethodName { get; }
    public ThrownExceptionInfo[] ThrownExceptions { get; }
}
