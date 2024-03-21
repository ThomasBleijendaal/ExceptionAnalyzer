using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

// TODO: expand a lot
public class HierarchyThrows
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
            try {
                throw new Exception();
            } catch (NotSupportedException) {
                throw;
            }
        }
    }
}",
CodeHelper.CreateException(1, "Exception"),
CodeHelper.CreateMethodExceptions(exceptions: new[] { (false, "System.NotSupportedException"), (true, "System.Exception") }));
    }
}
