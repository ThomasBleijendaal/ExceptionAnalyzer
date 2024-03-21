using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class ClassThrows
{
    [Test]
    public void Throw()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = new C();
            x.Throw();
        }
    }

    public class C {
        public void Throw() {
            throw new NotSupportedException();
        }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    [Test]
    public void ThrowOverload()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = new C();
            x.Throw(string.Empty);
        }
    }

    public class C {
        public void Throw() {
            throw new NotSupportedException();
        }
        public void Throw(string x) {
            throw new InvalidOperationException();
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }
}
