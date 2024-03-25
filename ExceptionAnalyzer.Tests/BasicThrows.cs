using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class BasicThrows
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
            throw new Exception();
        }
    }
}",
CodeHelper.CreateException(1),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowMultiple()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            if (false == true) {
                throw new InvalidOperationException();
            } else {
                throw new Exception();
            }
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateException(2),
CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.InvalidOperationException", "System.Exception" }));
    }

    [Test]
    public void ThrowIf()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowIfs()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateException(2, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.InvalidOperationException", "System.NotSupportedException" }));
    }

    [Test]
    public void ThrowTernary()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = (true == false) ? 1 : throw new Exception();
        }
    }
}",
CodeHelper.CreateException(1),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowNull()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var x = null ?? throw new Exception();
        }
    }
}",
CodeHelper.CreateException(1),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowVariable()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var ex = new Exception();
            throw ex;
        }
    }
}",
CodeHelper.CreateMethodExceptionsWithoutCreateException(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowTernaryVariable()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            var ex = new Exception();
            var x = (true == false) ? 1 : throw ex;
        }
    }
}",
CodeHelper.CreateMethodExceptionsWithoutCreateException(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowCatch()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowCatchWithoutVariableRethrow()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                throw new InvalidOperationException();
            }
            catch (Exception) {
                throw;
            }
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowCatchRethrowMultiple()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                if (false == true) {
                    throw new InvalidOperationException();
                } else {
                    throw new Exception();
                }
            }
            catch (Exception ex) {
                throw;
            }
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateException(2),
CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.InvalidOperationException", "System.Exception" }));
    }

    [Test]
    public void ThrowCatchNewThrow()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowCatchThrow()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateMethodExceptionsWithoutCreateException(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowCatchThrowMultiple()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                if (false == true) {
                    throw new InvalidOperationException();
                } else {
                    throw new Exception();
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}",
CodeHelper.CreateMethodExceptionsWithoutCreateException(exceptions: new[] { "System.Exception" }));
    }

    [Test]
    public void ThrowCatchNestedNewThrow()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    [Test]
    public void ThrowCatchNestedNoTypes()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

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
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowCatchNestedCatch()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                throw new Exception();
            }
            catch {
                try {
                    try {
                        throw;
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
    }
}",
CodeHelper.CreateException(1, "Exception"),
CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowCatchNestedCatchAndThrow()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                throw new Exception();
            }
            catch {
                try {
                    throw new InvalidOperationException();
                }
                catch {
                    throw;
                }
            }
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Ignore("The code is still too stupid")]
    [Test] // TODO: the Exception insnt put in to the deep throw, making it do incorrect stuff
    public void ThrowCatchNestedCatchAndThrow2()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using ExceptionAnalyzer;

namespace A {
    public class B {
        [AddExceptions]
        public void Method() {
            try {
                throw new Exception();
            }
            catch {
                try {
                    try {
                        throw;
                    }
                    catch (Exception) {
                        throw new InvalidOperationException();
                    }
                }
                catch {
                    throw;
                }
            }
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }
}
