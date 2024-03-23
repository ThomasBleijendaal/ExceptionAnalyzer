using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Helpers;

internal static class ExceptionInfoHelper
{
    // TODO: this needs to filter duplicates
    public static IEnumerable<ExceptionInfo> Combine(IEnumerable<ExceptionInfo> thrownExceptions, ExceptionInfo originalThrownException)
    {
        foreach (var thrownException in thrownExceptions)
        {
            if (thrownException == ExceptionInfo.All)
            {
                yield return originalThrownException;
            }
            else
            {
                yield return thrownException;
            }
        }
    }

    // TODO: this needs to filter duplicates

    public static IEnumerable<ExceptionInfo> Combine(IEnumerable<ExceptionInfo> thrownExceptions, IEnumerable<CatchInfo> catches)
        => CombineExceptions(thrownExceptions, catches).Distinct();

    private static IEnumerable<ExceptionInfo> CombineExceptions(IEnumerable<ExceptionInfo> thrownExceptions, IEnumerable<CatchInfo> catches)
    {
        var caughtExceptions = catches
            .Select(@catch => (@catch,
                exceptions: thrownExceptions.Where(exception => CatchesException(@catch, exception)).ToArray()))
            .ToArray();

        var uncaughtExceptions = thrownExceptions
            .Except(caughtExceptions.SelectMany(x => x.exceptions))
            .ToArray();

        foreach (var (@catch, exceptions) in caughtExceptions)
        {
            if (exceptions.Length > 0)
            {
                foreach (var exception in exceptions)
                {
                    foreach (var retrownException in Combine(@catch.Block.ThrownExceptions, exception))
                    {
                        yield return retrownException;

                        //if (@catch.CaughtException == ExceptionInfo.All)
                        //{
                        //    yield return exception;
                        //}
                        //else
                        //{
                        //    yield return retrownException;
                        //}
                    }
                }
            }
            else // TODO: missing feature flag
            {
                foreach (var thrownException in @catch.Block.ThrownExceptions)
                {
                    if (thrownException == ExceptionInfo.All)
                    {
                        yield return @catch.CaughtException;
                    }
                    else
                    {
                        yield return thrownException;
                    }
                }
            }
        }

        foreach (var exception in uncaughtExceptions)
        {
            yield return exception;
        }
    }

    public static bool CatchesException(CatchInfo @catch, ExceptionInfo exception)
    {
        // this logic is too simplistic (missing exception hierarchies + when filter clauses)

        if (@catch.CaughtException == ExceptionInfo.All ||
            SymbolEqualityComparer.Default.Equals(@catch.CaughtException.Type, exception.Type))
        {
            return true;
        }

        return GetAllTypes(exception.Type).Any(baseType =>
            SymbolEqualityComparer.Default.Equals(@catch.CaughtException.Type, baseType));
    }

    private static IEnumerable<ITypeSymbol> GetAllTypes(ITypeSymbol? type)
    {
        if (type == null)
        {
            yield break;
        }

        yield return type;

        if (type.BaseType != null)
        {
            foreach (var baseType in GetAllTypes(type.BaseType))
            {
                yield return baseType;
            }
        }
    }
}
