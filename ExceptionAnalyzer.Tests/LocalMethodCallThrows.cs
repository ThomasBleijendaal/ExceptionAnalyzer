using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class LocalMethodCallThrows
{
    [Test]
    public void ThrowFunction()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            Function();

            bool Function() { throw new NotSupportedException(); }
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws NotSupportedException
}
");
    }

    [Test]
    public void ThrowStaticFunction()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            Function();

            static bool Function() { throw new NotSupportedException(); }
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws NotSupportedException
}
");
    }

    [Test]
    public void ThrowLambda()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var Function = () => throw new NotSupportedException();

            Function();
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws NotSupportedException
}
");
    }
}
