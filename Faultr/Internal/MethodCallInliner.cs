using Faultr.Internal.Extensions;
using Faultr.Internal.Helpers;
using Faultr.Internal.Models;
using Microsoft.CodeAnalysis;

namespace Faultr.Internal;

internal sealed class MethodCallInliner
{
    private readonly IReadOnlyList<MethodInfo> _methodInfos;
    private readonly MethodInfo _methodToInline;
    private readonly IReadOnlyList<InterfaceMethodInfo> _interfaceMethodInfos;

    private readonly List<CallInfo> _processedCalls;

    public MethodCallInliner(
        IReadOnlyList<MethodInfo> methodInfos,
        IReadOnlyList<InterfaceMethodInfo> interfaceMethodInfos,
        MethodInfo methodToInline)
    {
        _methodInfos = methodInfos;
        _methodToInline = methodToInline;
        _interfaceMethodInfos = interfaceMethodInfos;

        _processedCalls = new List<CallInfo>([new CallInfo(null, methodToInline.MethodName)]);
    }

    public MethodInfo InlineMethodCalls()
    {
        var resolvedMethodCalls = new MethodInfo(
            _methodToInline.Symbol,
            _methodToInline.MethodName,
            _methodToInline.ArgumentTypes,
            InlineMethodCalls(_methodToInline, _methodToInline.Block));

        return MethodInfoHelper.Flatten(resolvedMethodCalls);
    }

    private BlockInfo InlineMethodCalls(MethodInfo methodInfo, BlockInfo blockInfo)
    {
        var catchInfos = blockInfo.CatchInfos.Select(@catch => InlineMethodCalls(methodInfo, @catch)).ToList();

        var blockInfos = blockInfo.Blocks.Select(block => InlineMethodCalls(methodInfo, block)).ToList();

        foreach (var call in blockInfo.MethodCalls)
        {
            if (_processedCalls.Contains(call))
            {
                continue;
            }

            _processedCalls.Add(call);

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
                    var flattened = InlineMethodCalls(foundInfo, foundInfo.Block);
                    blockInfos.Add(flattened);
                }
                else
                {
                    var interfaceMethod = _interfaceMethodInfos.FirstOrDefault(x => x.Method == foundInfo);
                    if (interfaceMethod != null)
                    {
                        foreach (var implementorInfo in interfaceMethod.Implementors)
                        {
                            var flattened = InlineMethodCalls(implementorInfo, implementorInfo.Block);
                            blockInfos.Add(flattened);
                        }
                    }
                }
            }
        }

        return new BlockInfo(blockInfo.ThrownExceptions, blockInfos, catchInfos);
    }

    private CatchInfo InlineMethodCalls(MethodInfo methodInfo, CatchInfo @catch)
    {
        return new CatchInfo(@catch.CaughtException, InlineMethodCalls(methodInfo, @catch.Block));
    }
}
