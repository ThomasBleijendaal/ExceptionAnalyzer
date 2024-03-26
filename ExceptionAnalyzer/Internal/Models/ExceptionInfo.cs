using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Models;

internal class ExceptionInfo : IEquatable<ExceptionInfo>
{
    public static readonly ExceptionInfo All = new(null, null, null, null);

    public ExceptionInfo(
        ITypeSymbol? type,
        string? exceptionCreation,
        List<string>? usingDirectives,
        List<VariableInfo>? constructorVariables)
    {
        Type = type;
        ExceptionCreation = exceptionCreation;
        UsingDirectives = usingDirectives;
        ConstructorVariables = constructorVariables;
    }

    public ITypeSymbol? Type { get; set; }

    // TODO: group into single class?
    public string? ExceptionCreation { get; }
    public List<string>? UsingDirectives { get; }
    public List<VariableInfo>? ConstructorVariables { get; }

    public bool Equals(ExceptionInfo other)
        => SymbolEqualityComparer.Default.Equals(Type, other.Type) &&
            ExceptionCreation == other.ExceptionCreation &&
            (UsingDirectives == other.UsingDirectives ||
            UsingDirectives.SequenceEqual(other.UsingDirectives));
}
