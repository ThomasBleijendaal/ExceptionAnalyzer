using System.CodeDom.Compiler;
using System.Text;
using ExceptionAnalyzer.Internal.Extensions;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis.Text;

namespace ExceptionAnalyzer.Internal;

internal sealed class SourceBuilder
{
    private readonly IEnumerable<MethodInfo> _methods;

    public SourceBuilder(IEnumerable<MethodInfo> methods)
    {
        _methods = methods;
    }

    public IEnumerable<(string, SourceText)> GenerateSourceText()
    {
        foreach (var method in _methods)
        {
            using var writer = new StringWriter();
            using var indentWriter = new IndentedTextWriter(writer, "    ");

            indentWriter.WriteLine("using System;");
            indentWriter.WriteLine();
            indentWriter.WriteLine($"namespace {method.Symbol.ContainingNamespace.Name}");
            using (indentWriter.Braces())
            {
                indentWriter.WriteLine($"// {method.Symbol.Name}.{method.MethodName} throws {string.Join(", ", method.Block.ThrownExceptions.Select(x => x.TypeName))}");
            }

            yield return ($"{method.Symbol.ContainingNamespace.Name}.{method.Symbol.Name}.{method.MethodName}.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
        }
    }
}
