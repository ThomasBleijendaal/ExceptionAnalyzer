using Faultr.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Faultr.Internal;

internal sealed class DeclarationParser
{
    private readonly GeneratorExecutionContext _context;
    private readonly MethodReceiver _receiver;

    public DeclarationParser(
        GeneratorExecutionContext context,
        MethodReceiver receiver)
    {
        _context = context;
        _receiver = receiver;
    }

    public IEnumerable<MethodInfo> ParseProperty(PropertyDeclarationSyntax candidate)
    {
        if (candidate.AccessorList != null)
        {
            foreach (var accessor in candidate.AccessorList.Accessors)
            {
                if (accessor.Body != null || accessor.ExpressionBody != null)
                {
                    var visitor = new ThrowExceptionVisitor(_context.Compilation);

                    accessor.Body?.Accept(visitor);
                    accessor.ExpressionBody?.Accept(visitor);

                    var info = visitor.Blocks.Pop();

                    var model = _context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                    var type = model.GetDeclaredSymbol(candidate);

                    if (type != null)
                    {
                        var methodInfo = new MethodInfo(
                            type.ContainingSymbol,
                            $"{accessor.Keyword.Text}_{type.Name}",
                            [],
                            info);

                        yield return methodInfo;
                    }
                }

            }
        }
    }

    public MethodInfo? ParseMethod(MethodDeclarationSyntax candidate)
    {
        BlockInfo blockInfo;

        if (candidate.Body != null)
        {
            var visitor = new ThrowExceptionVisitor(_context.Compilation);
            candidate.Body.Accept(visitor);

            blockInfo = visitor.Blocks.Pop();
        }
        else
        {
            blockInfo = new();
        }

        var model = _context.Compilation.GetSemanticModel(candidate.SyntaxTree);
        var type = model.GetDeclaredSymbol(candidate);

        if (type != null)
        {
            var methodInfo = new MethodInfo(
                type.ContainingSymbol,
                type.Name,
                type.Parameters.Select(x => x.Type).ToArray(),
                blockInfo)
            {
                HasAnnotation = _receiver.AttributedMethodCandidates.Contains(candidate),
                IsInterfaceMethod = candidate.Body == null
            };

            return methodInfo;
        }

        return null;
    }
}
