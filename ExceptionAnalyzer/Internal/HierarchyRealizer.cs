using ExceptionAnalyzer.Internal.Extensions;
using ExceptionAnalyzer.Internal.Helpers;
using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal;

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
