using Faultr.Tests.Helpers;
using NUnit.Framework;

namespace Faultr.Tests;

public class LambdaThrows
{
    [Test]
    public void Throw()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        Helper.DoStuff(() => throw new InvalidOperationException());
                    }
                }

                public static class Helper {
                    public static void DoStuff(Action action) {
                        action.Invoke();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }
}
