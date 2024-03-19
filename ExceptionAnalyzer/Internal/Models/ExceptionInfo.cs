using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Models;

internal class ExceptionInfo
{
    public static readonly ExceptionInfo All = new(null, null);

    public ExceptionInfo(ITypeSymbol? type, string? exceptionCreation)
    {
        Type = type;
        ExceptionCreation = exceptionCreation;
    }

    public ITypeSymbol? Type { get; set; }
    public string? ExceptionCreation { get; }
}
