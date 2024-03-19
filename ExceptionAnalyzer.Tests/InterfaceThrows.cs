using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class InterfaceThrows
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
            I x = new C();
            x.Throw();
        }
    }

    public interface I {
        void Throw();
    }
    public class C : I {
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
            I x = new C();
            x.Throw(string.Empty);
        }
    }

    public interface I {
        void Throw();
        void Throw(string x);
    }
    public class C : I {
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

    [Test]
    public void ThrowMultipleImplementations()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            I x = new C();
            x.Throw();
        }
    }

    public interface I {
        void Throw();
    }
    public class C1 : I {
        public void Throw() {
            throw new InvalidOperationException();
        }
    }
    public class C2 : I {
        public void Throw() {
            throw new NotSupportedException();
        }
    }
    public class C3 : I {
        public void Throw() {
            throw new ArgumentException();
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws InvalidOperationException, NotSupportedException, ArgumentException
}
");
    }
}
