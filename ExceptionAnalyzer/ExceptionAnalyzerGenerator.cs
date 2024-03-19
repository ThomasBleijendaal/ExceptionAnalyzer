using System.Reflection;
using ExceptionAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer;

/*
 * TODOs
 * 
 * V Support overloads
 * - Support generic overloads
 * V Support lambdas
 * 1/2 Support properties
 * V Support local functions
 * - Support class method calls
 * -V Overloads
 * -- Properties on classes
 * -- Nested classes
 * - Support interface method calls
 * -V Overloads
 * -V Multiple implementors
 * -- Properties on interfaces
 * -- Implicitly defined method calls
 * -- Handle stuff like IDisposable method call that will trigger too many exceptions
 * - Support exception origin detection
 * - Support output the reason of the exception (a la stack trace)
 * - Exception hierarchies
 * - Export all exception details
 * 
 * 
 * PLAN
 * 
 * - Create a class somewhere with a partial method
 * - Populate that partial method with the exception data
 * - Allow using this method in like DocumentFilters for OpenApi stuff
 * - Make stuff structs and lower memory pressure
 * - Make parsing enabled by build flag so it only tanks performance when wanted
 */

[Generator]
public class ExceptionAnalyzerGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is MethodReceiver receiver)
        {
            var parser = new DeclarationParser(context, receiver);

            var foundProperties = receiver.PropertyCandidates.SelectMany(parser.ParseProperty).OfType<MethodInfo>().ToList();

            var foundMethods = receiver.MethodCandidates.Select(parser.ParseMethod).OfType<MethodInfo>().ToList();

            var referenceData = foundProperties.Concat(foundMethods).ToList();

            var realizer = new HierarchyRealizer(referenceData);

            var filteredMethods = foundMethods
                .Where(x => x.HasAnnotation)
                .Select(realizer.RealizeMethodCalls)
                .Where(x => x.Block.ThrownExceptions.Count > 0)
                .ToList();

            var builder = new SourceBuilder(filteredMethods);

            foreach (var (name, source) in builder.GenerateSourceText())
            {
                context.AddSource(name, source);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MethodReceiver());

        //#if DEBUG
        //        if (!Debugger.IsAttached)
        //        {
        //            Debugger.Launch();
        //        }
        //#endif
    }
}

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

// TODO: name can be better
internal sealed class HierarchyRealizer
{
    private readonly IReadOnlyList<MethodInfo> _methodInfos;

    private readonly IReadOnlyList<InterfaceMethodInfo> _interfaceMethodInfos;

    public HierarchyRealizer(IReadOnlyList<MethodInfo> methodInfos)
    {
        _methodInfos = methodInfos;
        _interfaceMethodInfos = CalculateInterfaceMethods();
    }

    public MethodInfo RealizeMethodCalls(MethodInfo methodInfo)
    {
        var resolvedMethodCalls = new MethodInfo(methodInfo.Symbol, methodInfo.MethodName, methodInfo.ArgumentTypes, RealizeMethodCalls(methodInfo, methodInfo.Block));

        return MethodInfoHelper.Flatten(resolvedMethodCalls);
    }

    private BlockInfo RealizeMethodCalls(MethodInfo methodInfo, BlockInfo blockInfo)
    {
        var catchInfos = blockInfo.CatchInfos.Select(@catch => RealizeMethodCalls(methodInfo, @catch)).ToList();

        var blockInfos = blockInfo.Blocks.Select(block => RealizeMethodCalls(methodInfo, block)).ToList();

        foreach (var call in blockInfo.MethodCalls)
        {
            // if the call.Symbol is null, its a call to a method / property on its own class
            var symbol = call.Symbol ?? methodInfo.Symbol;
            var methodName = call.MethodName;

            var foundInfo = _methodInfos.FirstOrDefault(x =>
                SymbolEqualityComparer.Default.Equals(x.Symbol, symbol) &&
                x.MethodName == methodName &&
                x.ArgumentTypes.AreEqual(call.ArgumentTypes));

            if (foundInfo != null)
            {
                if (!foundInfo.IsInterfaceMethod)
                {
                    var flattened = RealizeMethodCalls(foundInfo, foundInfo.Block);
                    blockInfos.Add(flattened);
                }
                else
                {
                    var interfaceMethod = _interfaceMethodInfos.FirstOrDefault(x => x.Method == foundInfo);
                    if (interfaceMethod != null)
                    {
                        foreach (var implementorInfo in interfaceMethod.Implementors)
                        {
                            var flattened = RealizeMethodCalls(implementorInfo, implementorInfo.Block);
                            blockInfos.Add(flattened);
                        }
                    }
                }
            }
        }

        return new BlockInfo(blockInfo.ThrownExceptions, blockInfos, catchInfos);
    }

    private CatchInfo RealizeMethodCalls(MethodInfo methodInfo, CatchInfo @catch)
    {
        return new CatchInfo(@catch.CaughtException, RealizeMethodCalls(methodInfo, @catch.Block));
    }

    private IReadOnlyList<InterfaceMethodInfo> CalculateInterfaceMethods()
    {
        var methods = new List<InterfaceMethodInfo>();

        foreach (var interfaceMethod in _methodInfos.Where(x => x.IsInterfaceMethod))
        {
            var implementors = _methodInfos.Where(method =>
                method.Symbol is ITypeSymbol type &&
                type.Interfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, interfaceMethod.Symbol)) &&
                interfaceMethod.MethodName == method.MethodName &&
                interfaceMethod.ArgumentTypes.AreEqual(method.ArgumentTypes)).ToArray();

            methods.Add(new InterfaceMethodInfo(interfaceMethod, implementors));
        }

        return methods;
    }
}

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

        caughtException ??= new ExceptionInfo(ExceptionInfo.All);

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
            Blocks.Peek().ThrownExceptions.Add(new ExceptionInfo(ExceptionInfo.All));
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
        if (expression is ObjectCreationExpressionSyntax createExceptionStatement &&
            createExceptionStatement.Type is IdentifierNameSyntax identifierName)
        {
            Blocks.Peek().ThrownExceptions.Add(new ExceptionInfo(identifierName.Identifier.Text));
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
                return new ExceptionInfo(type.Type.Name);
            }
        }

        return null;
    }
}

internal class ExceptionInfo
{
    public const string All = "*";

    // TODO: this misses namespace
    public ExceptionInfo(string typeName)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
    }

    public string TypeName { get; set; }
}

public class AddExceptionsAttribute : Attribute
{

}
