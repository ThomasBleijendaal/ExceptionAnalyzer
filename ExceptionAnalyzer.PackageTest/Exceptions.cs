namespace ExceptionAnalyzer;

public partial class Exceptions
{
    public static Exceptions Current => new();

    private MethodExceptionInfo[] _methods = Array.Empty<MethodExceptionInfo>();

    public Exceptions()
    {
        SetThrownExceptions();
    }

    public MethodExceptionInfo[] Methods => _methods;

    partial void SetThrownExceptions();
}
