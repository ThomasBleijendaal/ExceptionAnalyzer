using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class PropertyCallThrows
{
    [Test]
    public void ThrowGet()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = Value;
        }

        private string Value { get => throw new NotSupportedException(); }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    [Test]
    public void ThrowSet()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            Value = string.Empty;
        }

        private string Value { set => throw new NotSupportedException(); }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }
}
