using System.Linq;

namespace Faultr.Tests.Helpers;

public static class CodeHelper
{
    public static string CreateException(int index, string exceptionType = "Exception", string @namespace = "A")
        => CreateExceptionCreation(index, [$"return new {exceptionType}();"], @namespace);

    public static string CreateExceptionCreation(int index, string[] constructor, string @namespace = "A")
    {
        return $$"""
            using System;
            using Faultr;
            using {{@namespace}};

            namespace Faultr.Exception{{index}}
            {
                public static class ExceptionCreator
                {
                    public static Exception Create()
                    {
                        {{string.Join("""

                                                               
                                                   """, constructor)}}
                    }
                }
            }

            """;
    }

    public static string CreateMethodExceptionsWithoutCreateException(string @namespace = "A.B", string method = "Method", params string[] exceptions)
        => CreateMethodExceptions(@namespace, method, exceptions.Select(x => (false, x)).ToArray());

    public static string CreateMethodExceptions(string @namespace = "A.B", string method = "Method", params string[] exceptions)
        => CreateMethodExceptions(@namespace, method, exceptions.Select(x => (true, x)).ToArray());

    public static string CreateMethodExceptions(string @namespace = "A.B", string method = "Method", (bool hasCreator, string type)[] exceptions = null)
    {
        var i = 0;

        var exceptionString = string.Join(
            """

                                
            """,
            exceptions.Select(e => $"new ThrownExceptionInfo(typeof({e.type}), {(e.hasCreator ? $"Faultr.Exception{++i}.ExceptionCreator.Create()" : "null")}),"));

        return $$"""
            namespace Faultr
            {
                public partial class Exceptions
                {
                    partial void SetThrownExceptions()
                    {
                        _methods = new[]
                        {
                            new MethodExceptionInfo(typeof({{@namespace}}), "{{method}}", new[]
                            {
                                {{exceptionString}}
                            }),
                        };
                    }
                }
            }

            """;
    }
}
