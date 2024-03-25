using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class ExceptionCreationTests
{
    // TODO: convert to code helper

    [Test]
    public void ThrowMultiple()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            throw new Exception();
            throw new Exception(""Message"");
        }
    }
}",
@"using System;
using ExceptionAnalyzer;
using A;

namespace ExceptionAnalyzer.Exception1
{
    public static class ExceptionCreator
    {
        public static Exception Create()
        {
            return new Exception();
        }
    }
}
",
@"using System;
using ExceptionAnalyzer;
using A;

namespace ExceptionAnalyzer.Exception2
{
    public static class ExceptionCreator
    {
        public static Exception Create()
        {
            return new Exception(""Message"");
        }
    }
}
",
@"namespace ExceptionAnalyzer
{
    public partial class Exceptions
    {
        partial void SetThrownExceptions()
        {
            Methods = new[]
            {
                new MethodExceptionInfo(typeof(A.B), ""Method"", new[]
                {
                    new ThrownExceptionInfo(typeof(System.Exception), ExceptionAnalyzer.Exception1.ExceptionCreator.Create()),
                    new ThrownExceptionInfo(typeof(System.Exception), ExceptionAnalyzer.Exception2.ExceptionCreator.Create()),
                }),
            };
        }
    }
}
");
    }

    [Test]
    public void ThrowWithConstants()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class Constant { public const string Message = ""Message"" }
    public class B {
        [AddExceptions]
        public void Method() {
            throw new Exception(Constant.Message);
        }
    }
}",
@"using System;
using ExceptionAnalyzer;
using A;

namespace ExceptionAnalyzer.Exception1
{
    public static class ExceptionCreator
    {
        public static Exception Create()
        {
            return new Exception(Constant.Message);
        }
    }
}
",
@"namespace ExceptionAnalyzer
{
    public partial class Exceptions
    {
        partial void SetThrownExceptions()
        {
            Methods = new[]
            {
                new MethodExceptionInfo(typeof(A.B), ""Method"", new[]
                {
                    new ThrownExceptionInfo(typeof(System.Exception), ExceptionAnalyzer.Exception1.ExceptionCreator.Create()),
                }),
            };
        }
    }
}
");
    }

    [Test]
    public void ThrowWithNestedNamespace()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    namespace Q {
        public class Constant { public const string Message = ""Message"" }
        public class B {
            [AddExceptions]
            public void Method() {
                throw new Exception(Constant.Message);
            }
        }
    }
}",
@"using System;
using ExceptionAnalyzer;
using A.Q;

namespace ExceptionAnalyzer.Exception1
{
    public static class ExceptionCreator
    {
        public static Exception Create()
        {
            return new Exception(Constant.Message);
        }
    }
}
",
@"namespace ExceptionAnalyzer
{
    public partial class Exceptions
    {
        partial void SetThrownExceptions()
        {
            Methods = new[]
            {
                new MethodExceptionInfo(typeof(A.Q.B), ""Method"", new[]
                {
                    new ThrownExceptionInfo(typeof(System.Exception), ExceptionAnalyzer.Exception1.ExceptionCreator.Create()),
                }),
            };
        }
    }
}
");
    }
}
