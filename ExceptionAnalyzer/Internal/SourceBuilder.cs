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

            //indentWriter.WriteLine("using System;");
            //indentWriter.WriteLine();
            //indentWriter.WriteLine($"namespace {method.Symbol.ContainingNamespace.Name}");
            //using (indentWriter.Braces())
            //{
            //    indentWriter.WriteLine($"// {method.Symbol.Name}.{method.MethodName} throws {string.Join(", ", method.Block.ThrownExceptions.Select(x => x.Type?.Name))}");
            //}

            indentWriter.WriteLine($"namespace {nameof(ExceptionAnalyzer)}");
            using (indentWriter.Braces())
            {
                indentWriter.WriteLine($"public partial class {nameof(Exceptions)}");
                using (indentWriter.Braces())
                {
                    indentWriter.WriteLine($"partial void SetThrownExceptions()");
                    using (indentWriter.Braces())
                    {
                        indentWriter.WriteLine($"Methods = new[]");
                        using (indentWriter.Braces())
                        {
                            indentWriter.Write($"new MethodExceptionInfo(");
                            indentWriter.Write($"typeof({method.Symbol.ContainingNamespace}.{method.Symbol.Name}), ");
                            indentWriter.WriteLine($"\"{method.MethodName}\", new[]");
                            using (indentWriter.Braces())
                            {
                                foreach (var exception in method.Block.ThrownExceptions.Where(x => x.Type != null))
                                {
                                    indentWriter.Write($"new ThrownExceptionInfo(");
                                    indentWriter.Write($"typeof({exception.Type!.ContainingNamespace}.{exception.Type.Name}), ");
                                    if (!string.IsNullOrEmpty(exception.ExceptionCreation))
                                    {
                                        indentWriter.Write(exception.ExceptionCreation);
                                        indentWriter.WriteLine("),");
                                    }
                                    else
                                    {
                                        indentWriter.WriteLine("null),");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            yield return ($"{method.Symbol.ContainingNamespace.Name}.{method.Symbol.Name}.{method.MethodName}.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
        }
    }
}
