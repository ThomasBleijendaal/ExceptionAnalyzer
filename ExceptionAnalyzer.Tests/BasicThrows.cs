using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class BasicThrows
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
    public void ThrowIf()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            if (true == false) {
                throw new Exception();
            }
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
    public void ThrowIfs()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            if (true == false) {
                throw new InvalidOperationException();
            }
            if (true == false) {
                throw new NotSupportedException();
            }
        }
    }
}",
@"using System;

namespace A
{
    // B.Method throws InvalidOperationException, NotSupportedException
}
");
    }

    [Test]
    public void ThrowTernary()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = (true == false) ? 1 : throw new Exception();
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
    public void ThrowNull()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = null ?? throw new Exception();
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
    public void ThrowVariable()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var ex = new Exception();
            throw ex;
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
    public void ThrowTernaryVariable()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var ex = new Exception();
            var x = (true == false) ? 1 : throw ex;
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
    public void ThrowCatch()
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
            }
        }
    }
}");
    }

    [Test]
    public void ThrowCatchRethrow()
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
                throw;
            }
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
    public void ThrowCatchNewThrow()
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
                throw new InvalidOperationException();
            }
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
    public void ThrowCatchThrow()
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
                throw ex;
            }
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
    public void ThrowCatchNestedNewThrow()
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
                try {
                    throw new InvalidOperationException();
                }
                catch (InvalidOperationException ex) {
                    throw new NotSupportedException();
                }
            }
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
    public void ThrowCatchNestedNoTypes()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using OpenApiGenerator;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                try {
                    try {
                        throw new InvalidOperationException();
                    }
                    catch {
                        throw;
                    }
                }
                catch {
                    throw;
                }
            }
            catch {
                throw;
            }
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
