using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class ClassThrows
{
    [Test]
    public void Throw()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

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
@"using System;

namespace A
{
    // B.Method throws NotSupportedException
}
");
    }

    [Test]
    public void ThrowOverload()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

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
@"using System;

namespace A
{
    // B.Method throws InvalidOperationException
}
");
    }
}
