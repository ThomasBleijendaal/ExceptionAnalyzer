namespace Faultr.Internal.Models;

internal class InterfaceMethodInfo
{
    public InterfaceMethodInfo(MethodInfo method, IReadOnlyList<MethodInfo> implementors)
    {
        Method = method;
        Implementors = implementors;
    }

    public MethodInfo Method { get; }
    public IReadOnlyList<MethodInfo> Implementors { get; }
}
