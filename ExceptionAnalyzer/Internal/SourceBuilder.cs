using System.CodeDom.Compiler;
using System.Text;
using ExceptionAnalyzer.Internal.Extensions;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis.Text;

namespace ExceptionAnalyzer.Internal;

internal sealed class SourceBuilder
{
    private readonly IReadOnlyList<MethodInfo> _methods;

    public SourceBuilder(IReadOnlyList<MethodInfo> methods)
    {
        _methods = methods;
    }

    public IEnumerable<(string, SourceText)> GenerateSourceText()
    {
        if (_methods.Count == 0)
        {
            yield break;
        }

        var i = 0;

        using var writer = new StringWriter();
        using var indentWriter = new IndentedTextWriter(writer, "    ");

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
                    using (indentWriter.BracesWithSemiColon())
                    {
                        foreach (var method in _methods)
                        {
                            indentWriter.Write($"new MethodExceptionInfo(");
                            indentWriter.Write($"typeof({method.Symbol.ToDisplayString()}), ");
                            indentWriter.WriteLine($"\"{method.MethodName}\", new[]");
                            using (indentWriter.BracesWithComma())
                            {
                                foreach (var exception in method.Block.ThrownExceptions.Where(x => x.Type != null))
                                {
                                    indentWriter.Write($"new ThrownExceptionInfo(");
                                    indentWriter.Write($"typeof({exception.Type!.ToDisplayString()}), ");
                                    if (string.IsNullOrEmpty(exception.ExceptionCreation))
                                    {
                                        indentWriter.WriteLine("null),");
                                    }
                                    else
                                    {
                                        ++i;

                                        var exceptionName = $"Exception{i}";

                                        indentWriter.WriteLine($"{nameof(ExceptionAnalyzer)}.{exceptionName}.ExceptionCreator.Create()),");

                                        using var exceptionWriter = new StringWriter();
                                        using var exceptionIndentWriter = new IndentedTextWriter(exceptionWriter, "    ");

                                        if (exception.UsingDirectives != null)
                                        {
                                            foreach (var @using in exception.UsingDirectives)
                                            {
                                                exceptionIndentWriter.WriteLine(@using);
                                            }
                                        }

                                        exceptionIndentWriter.WriteLine($"using {method.Symbol.ContainingNamespace.ToDisplayString()};");
                                        exceptionIndentWriter.WriteLine();

                                        exceptionIndentWriter.WriteLine($"namespace {nameof(ExceptionAnalyzer)}.{exceptionName}");
                                        using (exceptionIndentWriter.Braces())
                                        {
                                            exceptionIndentWriter.WriteLine("public static class ExceptionCreator");
                                            using (exceptionIndentWriter.Braces())
                                            {
                                                exceptionIndentWriter.WriteLine("public static Exception Create()");
                                                using (exceptionIndentWriter.Braces())
                                                {
                                                    exceptionIndentWriter.Write("return ");
                                                    exceptionIndentWriter.Write(exception.ExceptionCreation);
                                                    exceptionIndentWriter.WriteLine(";");
                                                }
                                            }
                                        }

                                        yield return ($"ExceptionAnalyzer.Exceptions.{i}.g.cs", SourceText.From(exceptionWriter.ToString(), Encoding.UTF8));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        yield return ($"ExceptionAnalyzer.Exceptions.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
    }
}
