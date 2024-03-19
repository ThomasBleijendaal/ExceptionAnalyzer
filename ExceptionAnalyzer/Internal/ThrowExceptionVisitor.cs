using System.Reflection;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer.Internal;

internal sealed class ThrowExceptionVisitor : CSharpSyntaxWalker
{
    private readonly Compilation _compilation;

    public Stack<BlockInfo> Blocks { get; set; } = new();

    private readonly Stack<CallInfo> _methodCalls = new();

    public ThrowExceptionVisitor(Compilation compilation)
    {
        Console.WriteLine("-----------");

        _compilation = compilation;
        Blocks.Push(new());
    }

    public override void Visit(SyntaxNode? node)
    {
        Console.WriteLine(node.GetType().Name);

        base.Visit(node);
    }

    public override void VisitTryStatement(TryStatementSyntax node)
    {
        Blocks.Push(new());

        base.VisitTryStatement(node);

        var tryInfo = Blocks.Pop();

        Blocks.Peek().Blocks.Add(tryInfo);
    }

    public override void VisitCatchClause(CatchClauseSyntax node)
    {
        ExceptionInfo? caughtException = null;

        if (node.Declaration?.Type is IdentifierNameSyntax identifierName &&
            GetExceptionInfo(identifierName) is { } exception)
        {
            caughtException = exception;
        }

        caughtException ??= ExceptionInfo.All;

        Blocks.Push(new());

        base.VisitCatchClause(node);

        var catchInfo = Blocks.Pop();

        var data = new CatchInfo(caughtException, catchInfo);

        Blocks.Peek().CatchInfos.Add(data);
    }

    public override void VisitThrowExpression(ThrowExpressionSyntax node)
    {
        ParseObjectCreationExpression(node.Expression);
        ParseIdentifierExpression(node.Expression);

        base.VisitThrowExpression(node);
    }

    public override void VisitThrowStatement(ThrowStatementSyntax node)
    {
        if (node.Expression == null)
        {
            Blocks.Peek().ThrownExceptions.Add(ExceptionInfo.All);
        }
        else
        {
            ParseObjectCreationExpression(node.Expression);
            ParseIdentifierExpression(node.Expression);
        }

        base.VisitThrowStatement(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax identifier)
        {
            _methodCalls.Push(new CallInfo(null, identifier.Identifier.Text));
        }

        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var objectIdentifier = memberAccess.Expression;

            var model = _compilation.GetSemanticModel(objectIdentifier.SyntaxTree);
            var type = model.GetTypeInfo(objectIdentifier);

            if (type.Type != null)
            {
                _methodCalls.Push(new CallInfo(type.Type, memberAccess.Name.Identifier.Text));
            }
        }

        base.VisitInvocationExpression(node);

        Blocks.Peek().MethodCalls.Add(_methodCalls.Pop());
    }

    public override void VisitArgumentList(ArgumentListSyntax node)
    {
        if (_methodCalls.Count > 0)
        {
            var model = _compilation.GetSemanticModel(node.SyntaxTree);

            foreach (var type in node.Arguments.Select(x => model.GetTypeInfo(x.Expression)))
            {
                _methodCalls.Peek().ArgumentTypes.Add(type.Type);
            }
        }

        base.VisitArgumentList(node);
    }

    public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
    {
        // read potential property
        // TODO: verify whether this is robust enough
        if (node.Value is IdentifierNameSyntax identifier)
        {
            Blocks.Peek().MethodCalls.Add(new CallInfo(null, $"get_{identifier.Identifier.Text}"));
        }

        base.VisitEqualsValueClause(node);
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        // write potential property
        // TODO: verify whether this is robust enough
        if (node.Left is IdentifierNameSyntax identifier)
        {
            Blocks.Peek().MethodCalls.Add(new CallInfo(null, $"set_{identifier.Identifier.Text}"));
        }

        base.VisitAssignmentExpression(node);
    }

    private void ParseObjectCreationExpression(ExpressionSyntax? expression)
    {
        if (expression is ObjectCreationExpressionSyntax createExceptionStatement)
        {
            var model = _compilation.GetSemanticModel(createExceptionStatement.SyntaxTree);
            var type = model.GetTypeInfo(createExceptionStatement);

            if (type.Type != null)
            {
                Blocks.Peek().ThrownExceptions.Add(new ExceptionInfo(type.Type, createExceptionStatement.ToString()));
            }
        }
    }

    private void ParseIdentifierExpression(ExpressionSyntax? expression)
    {
        if (GetExceptionInfo(expression) is { } exception)
        {
            Blocks.Peek().ThrownExceptions.Add(exception);
        }
    }

    private ExceptionInfo? GetExceptionInfo(ExpressionSyntax? expression)
    {
        if (expression is IdentifierNameSyntax identifierName)
        {
            var model = _compilation.GetSemanticModel(identifierName.SyntaxTree);
            var type = model.GetTypeInfo(identifierName);

            if (type.Type != null)
            {
                return new ExceptionInfo(type.Type, null);
            }
        }

        return null;
    }
}
