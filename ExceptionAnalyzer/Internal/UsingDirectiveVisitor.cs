using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer.Internal;

internal sealed class UsingDirectiveVisitor : CSharpSyntaxWalker
{
    public List<string> NamespacesDirectives { get; set; } = new();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        NamespacesDirectives.Add(node.ToString());

        base.VisitUsingDirective(node);
    }
}
