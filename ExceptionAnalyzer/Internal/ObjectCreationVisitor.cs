using System.Reflection;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer.Internal;

internal sealed class ObjectCreationVisitor : CSharpSyntaxWalker
{
    private readonly SemanticModel _model;

    public List<VariableInfo> Variables { get; } = new();

    public ObjectCreationVisitor(SemanticModel model)
    {
        _model = model;
    }

    public override void VisitArgument(ArgumentSyntax node)
    {
        // TODO: this exclusion might not be enough
        if (node.Expression is not MemberAccessExpressionSyntax)
        {
            base.VisitArgument(node);
        }
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (_model.GetTypeInfo(node).Type is { } type)
        {
            Variables.Add(new VariableInfo(type, node.Identifier.Text));
        }

        base.VisitIdentifierName(node);
    }
}
