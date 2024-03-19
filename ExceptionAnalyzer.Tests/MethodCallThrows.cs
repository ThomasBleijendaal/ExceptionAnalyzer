using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class MethodCallThrows
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
            SomeMethod();
        }

        private void SomeMethod() {
            throw new Exception();
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws Exception
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
            SomeMethod(1);
        }

        private void SomeMethod() {
            throw new Exception();
        }

        private void SomeMethod(int a) {
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
    public void ThrowFromNestedMethod()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            SomeMethodA();
        }

        private void SomeMethodA() {
            SomeMethodB();
        }

        private void SomeMethodB() {
            throw new Exception();
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws Exception
}
");
    }

    [Test]
    public void ThrowFromCatch()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                throw new Exception();
            }
            catch (Exception ex) {
                SomeMethod();
            }
        }

        private void SomeMethod() {
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
    public void ThrowFromNestedCatchWithImpossibleException()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                try {
                    SomeMethod();
                }
                catch (NotSupportedException ex) {
                    throw new NullReferenceException();
                }
            }
            catch (Exception ex) {
                throw;
            }
        }

        private void SomeMethod() {
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
    public void ThrowFromNestedCatch()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                try {
                    SomeMethod();
                }
                catch (InvalidOperationException ex) {
                    throw new NullReferenceExecption();
                }
            }
            catch (Exception ex) {
                throw;
            }
        }

        private void SomeMethod() {
            throw new InvalidOperationException();
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws NullReferenceExecption
}
");
    }
}
