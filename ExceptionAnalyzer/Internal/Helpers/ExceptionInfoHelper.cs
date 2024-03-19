using ExceptionAnalyzer.Internal.Models;
using Microsoft.CodeAnalysis;

namespace ExceptionAnalyzer.Internal.Helpers;

internal static class ExceptionInfoHelper
{
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

    public static IEnumerable<ExceptionInfo> Combine(IEnumerable<ExceptionInfo> thrownExceptions, IEnumerable<CatchInfo> catches)
    {
        foreach (var thrownException in thrownExceptions)
        {
            var caught = false;

            foreach (var @catch in catches)
            {
                // this logic is too simplistic (missing exception hierarchies + filter clauses)
                if (SymbolEqualityComparer.Default.Equals(@catch.CaughtException.Type, thrownException.Type) ||
                    // TODO: restore
                    //@catch.CaughtException.TypeName == "Exception" ||
                    @catch.CaughtException == ExceptionInfo.All)
                {
                    caught = true;

                    foreach (var retrownException in Combine(@catch.Block.ThrownExceptions, thrownException))
                    {
                        if (@catch.CaughtException == ExceptionInfo.All)
                        {
                            yield return thrownException;
                        }
                        else
                        {
                            yield return retrownException;
                        }
                    }
                }
            }

            if (!caught)
            {
                yield return thrownException;
            }
        }
    }
}
