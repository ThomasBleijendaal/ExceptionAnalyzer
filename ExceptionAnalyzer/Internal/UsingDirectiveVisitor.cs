using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer.Internal;

internal sealed class UsingDirectiveVisitor : CSharpSyntaxWalker
{
    private readonly SemanticModel _model;

    public UsingDirectiveVisitor(SemanticModel model)
    {
        _model = model;
    }

    public List<string> NamespacesDirectives { get; } = new();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        NamespacesDirectives.Add(node.ToString());

        base.VisitUsingDirective(node);
    }
}
