using Faultr.Internal;
using Faultr.Internal.Helpers;
using Faultr.Internal.Models;
using Microsoft.CodeAnalysis;

namespace Faultr;

/*
 * TODOs
 * 
 * V Support overloads
 * - Support generic overloads
 * - Support generic types
 * - Async
 * V- Basic support
 * -- Correct await throw
 * - Global scope
 * - Static
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
 * -- Interfaces on interfaces
 * - Support output the reason of the exception (a la stack trace - but then explain where exception came from)
 * - Exception hierarchies
 * -V Basic inheritance
 * -- More?
 * - When expression
 * - Export all exception details (initializer etc)
 * V Fallback when exception creation uses variable (just insert default / null)
 * - Global usings
 * - Detect cycle
 * 
 * PLAN
 * 
 * V Create a class somewhere with a partial method
 * V Populate that partial method with the exception data
 * - Allow using this method in like DocumentFilters for OpenApi stuff
 * - Make stuff structs and lower memory pressure (measure first)
 * - Make parsing enabled by build flag so it only tanks performance when wanted
 * 
 * DEBT
 * 
 * - Codegen should be using roslyn features and not a whole lot of string concats
 * 
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
            var interfaceMethodInfo = MethodInfoHelper.CalculateInterfaceMethods(referenceData);

            var filteredMethods = foundMethods
                .Where(x => x.HasAnnotation)
                .Select(method =>
                {
                    var inliner = new MethodCallInliner(referenceData, interfaceMethodInfo, method);
                    return inliner.InlineMethodCalls();
                })
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
