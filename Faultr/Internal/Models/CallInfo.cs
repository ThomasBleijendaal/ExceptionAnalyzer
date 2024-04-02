using Faultr.Internal.Extensions;
using Microsoft.CodeAnalysis;

namespace Faultr.Internal.Models;

internal class CallInfo : IEquatable<CallInfo>
{
    public CallInfo(ITypeSymbol? type, string methodName)
    {
        Symbol = type;
        MethodName = methodName;
        ArgumentTypes = new();
    }

    public ITypeSymbol? Symbol { get; }
    public string MethodName { get; }
    public List<ITypeSymbol?> ArgumentTypes { get; }

    public bool Equals(CallInfo other) =>
        SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol) &&
        MethodName == other.MethodName &&
        ArgumentTypes.AreEqual(other.ArgumentTypes);
}
