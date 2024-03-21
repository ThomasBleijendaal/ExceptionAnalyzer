using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Models;

internal class ExceptionInfo
{
    public static readonly ExceptionInfo All = new(null, null, null);

    public ExceptionInfo(
        ITypeSymbol? type,
        string? exceptionCreation,
        List<string>? usingDirectives)
    {
        Type = type;
        ExceptionCreation = exceptionCreation;
        UsingDirectives = usingDirectives;
    }

    public ITypeSymbol? Type { get; set; }
    public string? ExceptionCreation { get; }
    public List<string>? UsingDirectives { get; }
}
