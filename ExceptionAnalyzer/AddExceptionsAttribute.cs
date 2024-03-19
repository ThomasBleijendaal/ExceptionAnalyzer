namespace ExceptionAnalyzer;

public class AddExceptionsAttribute : Attribute
{

}

public partial class Exceptions
{
    public static Exceptions Current => new();

    public MethodExceptionInfo[] Methods = Array.Empty<MethodExceptionInfo>();

    public Exceptions()
    {
        SetThrownExceptions();
    }

    partial void SetThrownExceptions();
}

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
