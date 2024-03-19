using System.CodeDom.Compiler;
using System.Text;
using Faultr.Internal.Extensions;
using Faultr.Internal.Models;
using Microsoft.CodeAnalysis.Text;

namespace Faultr.Internal;

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

        indentWriter.WriteLine($"namespace {nameof(Faultr)}");
        using (indentWriter.Braces())
        {
            indentWriter.WriteLine("public partial class Exceptions");
            using (indentWriter.Braces())
            {
                indentWriter.WriteLine("partial void SetThrownExceptions()");
                using (indentWriter.Braces())
                {
                    indentWriter.WriteLine($"_methods = new[]");
                    using (indentWriter.BracesWithSemiColon())
                    {
                        foreach (var method in _methods)
                        {
                            indentWriter.Write("new MethodExceptionInfo(");
                            indentWriter.Write($"typeof({method.Symbol.ToDisplayString()}), ");
                            indentWriter.WriteLine($"\"{method.MethodName}\", new[]");
                            using (indentWriter.Braces("),"))
                            {
                                foreach (var exception in method.Block.ThrownExceptions.Where(x => x.Type != null))
                                {
                                    indentWriter.Write("new ThrownExceptionInfo(");
                                    indentWriter.Write($"typeof({exception.Type!.ToDisplayString()}), ");
                                    if (string.IsNullOrEmpty(exception.ExceptionCreation))
                                    {
                                        indentWriter.WriteLine("null),");
                                    }
                                    else
                                    {
                                        ++i;

                                        var exceptionName = $"Exception{i}";

                                        indentWriter.WriteLine($"{nameof(Faultr)}.{exceptionName}.ExceptionCreator.Create()),");

                                        using var exceptionWriter = new StringWriter();
                                        using var exceptionIndentWriter = new IndentedTextWriter(exceptionWriter, "    ");

                                        if (exception.UsingDirectives != null)
                                        {
                                            foreach (var @using in exception.UsingDirectives)
                                            {
                                                exceptionIndentWriter.WriteLine(@using);
                                            }
                                        }

                                        if (!method.Symbol.ContainingNamespace.IsGlobalNamespace)
                                        {
                                            exceptionIndentWriter.WriteLine($"using {method.Symbol.ContainingNamespace.ToDisplayString()};");
                                            exceptionIndentWriter.WriteLine();
                                        }
                                        else
                                        {
                                            if (exception.UsingDirectives != null)
                                            {
                                                exceptionIndentWriter.WriteLine();
                                            }
                                        }

                                        exceptionIndentWriter.WriteLine($"namespace {nameof(Faultr)}.{exceptionName}");
                                        using (exceptionIndentWriter.Braces())
                                        {
                                            exceptionIndentWriter.WriteLine("public static class ExceptionCreator");
                                            using (exceptionIndentWriter.Braces())
                                            {
                                                exceptionIndentWriter.WriteLine("public static Exception Create()");
                                                using (exceptionIndentWriter.Braces())
                                                {
                                                    if (exception.ConstructorVariables != null)
                                                    {
                                                        foreach (var variable in exception.ConstructorVariables)
                                                        {
                                                            exceptionIndentWriter.WriteLine($"{variable.Type.ToDisplayString()} {variable.Name} = default!;");
                                                        }
                                                    }

                                                    exceptionIndentWriter.Write("return ");
                                                    exceptionIndentWriter.Write(exception.ExceptionCreation);
                                                    exceptionIndentWriter.WriteLine(";");
                                                }
                                            }
                                        }

                                        yield return ($"Faultr.Exceptions.{i}.g.cs", SourceText.From(exceptionWriter.ToString(), Encoding.UTF8));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        yield return ($"Faultr.Exceptions.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
    }
}
