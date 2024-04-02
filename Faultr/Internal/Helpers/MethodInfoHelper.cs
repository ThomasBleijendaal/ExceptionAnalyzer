using Faultr.Internal.Extensions;
using Faultr.Internal.Models;
using Microsoft.CodeAnalysis;

namespace Faultr.Internal.Helpers;

internal static class MethodInfoHelper
{
    public static MethodInfo Flatten(MethodInfo method)
    {
        var exceptions = ResolveExceptions(method.Block);

        return new MethodInfo(method.Symbol, method.MethodName, method.ArgumentTypes, new BlockInfo(exceptions));
    }

    public static IReadOnlyList<InterfaceMethodInfo> CalculateInterfaceMethods(IReadOnlyList<MethodInfo> methodInfos)
    {
        var methods = new List<InterfaceMethodInfo>();

        foreach (var interfaceMethod in methodInfos.Where(x => x.IsInterfaceMethod))
        {
            var implementors = methodInfos.Where(method =>
                method.Symbol is INamedTypeSymbol type &&
                GetAllInterfaces(type).Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, interfaceMethod.Symbol)) &&
                interfaceMethod.MethodName == method.MethodName &&
                interfaceMethod.ArgumentTypes.AreEqual(method.ArgumentTypes)).ToArray();

            methods.Add(new InterfaceMethodInfo(interfaceMethod, implementors));
        }

        return methods;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllInterfaces(INamedTypeSymbol symbol)
    {
        foreach (var @interface in symbol.Interfaces)
        {
            yield return @interface;

            foreach (var @interfaceInterface in GetAllInterfaces(@interface))
            {
                yield return interfaceInterface;
            }
        }
    }

    private static IReadOnlyList<ExceptionInfo> ResolveExceptions(BlockInfo block)
    {
        var exceptions = new List<ExceptionInfo>(block.ThrownExceptions);

        foreach (var nestedBlock in block.Blocks)
        {
            exceptions.AddRange(ResolveExceptions(nestedBlock));
        }

        return ResolveExceptions(exceptions, block.CatchInfos);
    }

    private static IReadOnlyList<ExceptionInfo> ResolveExceptions(
        IReadOnlyList<ExceptionInfo> exceptions,
        IReadOnlyList<CatchInfo> catches)
    {
        if (catches.Count == 0)
        {
            return exceptions;
        }

        var flattenedCatches = catches.Select(@catch =>
        {
            var exceptions = ResolveExceptions(@catch.Block);
            return new CatchInfo(@catch.CaughtException, new BlockInfo(exceptions));
        }).ToArray();

        return ExceptionInfoHelper.Combine(exceptions, flattenedCatches).ToArray();
    }
}
