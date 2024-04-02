using Faultr.Tests.Helpers;
using NUnit.Framework;

namespace Faultr.Tests;

public class AsyncThrows
{
    [Test]
    public void Throw()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using Faultr;

namespace A {
    public class B {
        [AddExceptions]
        public async Task Method() {
            throw new NotSupportedException();
        }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    [Test]
    public void ThrowMethod()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using Faultr;

namespace A {
    public class B {
        [AddExceptions]
        public async Task Method() {
            await Do();
        }

        private async Task Do() {
            throw new NotSupportedException();
        }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    [Test]
    public void ThrowAwait()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using Faultr;

namespace A {
    public class B {
        [AddExceptions]
        public async Task Method() {
            var task = Do();
            await task;
        }

        private async Task Do() {
            throw new NotSupportedException();
        }
    }
}",
CodeHelper.CreateException(1, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: "System.NotSupportedException"));
    }

    // TODO: the naive logic takes the safe route and returns both exceptions, but should only return InvalidOperationException
    // it does not look at the await instruction -- causing it to miss the try catch
    [Test]
    public void ThrowTryAwait()
    {
        GeneratorTestHelper.TestGeneratedCode(@"using System;
using Faultr;

namespace A {
    public class B {
        [AddExceptions]
        public async Task Method() {
            var task = Do();

            try {
                await task;
            }
            catch (Exception ex) {
                throw new InvalidOperationException();
            }
        }

        private async Task Do() {
            throw new NotSupportedException();
        }
    }
}",
CodeHelper.CreateException(1, "InvalidOperationException"),
CodeHelper.CreateException(2, "NotSupportedException"),
CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.InvalidOperationException", "System.NotSupportedException" }));
    }
}
