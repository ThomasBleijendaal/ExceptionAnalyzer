using System.Reflection;
using ExceptionAnalyzer.Internal;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;

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
