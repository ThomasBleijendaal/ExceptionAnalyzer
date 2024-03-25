using System.Linq;

namespace ExceptionAnalyzer.Tests.Helpers;

public static class CodeHelper
{
    public static string CreateException(int index, string exceptionType = "Exception", string @namespace = "A")
    {
        return
@$"using System;
using ExceptionAnalyzer;
using {@namespace};

namespace ExceptionAnalyzer.Exception{index}
{{
    public static class ExceptionCreator
    {{
        public static Exception Create()
        {{
            return new {exceptionType}();
        }}
    }}
}}
";
    }

    public static string CreateMethodExceptionsWithoutCreateException(string @namespace = "A.B", string method = "Method", params string[] exceptions)
        => CreateMethodExceptions(@namespace, method, exceptions.Select(x => (false, x)).ToArray());

    public static string CreateMethodExceptions(string @namespace = "A.B", string method = "Method", params string[] exceptions)
        => CreateMethodExceptions(@namespace, method, exceptions.Select(x => (true, x)).ToArray());

    public static string CreateMethodExceptions(string @namespace = "A.B", string method = "Method", (bool hasCreator, string type)[] exceptions = null)
    {
        var i = 0;

        var exceptionString = string.Join(
            "\r\n                    ",
            exceptions.Select(e => $"new ThrownExceptionInfo(typeof({e.type}), {(e.hasCreator ? $"ExceptionAnalyzer.Exception{++i}.ExceptionCreator.Create()" : "null")}),"));

        return
@$"namespace ExceptionAnalyzer
{{
    public partial class Exceptions
    {{
        partial void SetThrownExceptions()
        {{
            Methods = new[]
            {{
                new MethodExceptionInfo(typeof({@namespace}), ""{method}"", new[]
                {{
                    {exceptionString}
                }}),
            }};
        }}
    }}
}}
";
    }
}
