using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class InterfaceThrows
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
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowMultipleImplementations()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            I x = new C1();
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
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateException(2, "NotSupportedException"),
CodeHelper.CreateException(3, "ArgumentException"),
CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.InvalidOperationException", "System.NotSupportedException", "System.ArgumentException" }));
    }
}
