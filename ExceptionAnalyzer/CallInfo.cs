using ExceptionAnalyzer.Extensions;
using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer;

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
        ArgumentTypes.AreEqual(other.ArgumentTypes);
}
