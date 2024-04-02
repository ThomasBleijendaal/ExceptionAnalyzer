using Microsoft.CodeAnalysis;

namespace Faultr.Internal.Models;

internal class VariableInfo
{
    public VariableInfo(ITypeSymbol type, string name)
    {
        Type = type;
        Name = name;
    }

    public ITypeSymbol Type { get; set; }
    public string Name { get; set; }
}
