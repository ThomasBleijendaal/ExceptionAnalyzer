using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer;

internal class MethodInfo
{
    public MethodInfo(ISymbol symbol, string methodName, IReadOnlyList<ITypeSymbol?> argumentTypes, BlockInfo block)
    {
        Symbol = symbol;
        MethodName = methodName;
        ArgumentTypes = argumentTypes;
        Block = block;
    }

    public ISymbol Symbol { get; }
    public string MethodName { get; }
    public IReadOnlyList<ITypeSymbol?> ArgumentTypes { get; }
    public BlockInfo Block { get; }

    public bool HasAnnotation { get; set; }
    public bool IsInterfaceMethod { get; set; }
}
