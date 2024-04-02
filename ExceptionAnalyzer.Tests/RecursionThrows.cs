using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class RecursionThrows
{
    [Test]
    public void ThrowRecursiveMethod()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        throw new InvalidOperationException();
                        Method();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowRecursiveMethods()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        MethodA();
                    }

                    public void MethodA() {
                        MethodB();
                    }

                    public void MethodB() {
                        MethodC();
                    }

                    public void MethodC() {
                        throw new InvalidOperationException();
                        Method();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowRecursiveClass()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        var c = new C();
                        c.Method();
                    }
                }
                public class C {
                    public void Method() {
                        throw new InvalidOperationException();
                        Method();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowRecursiveInterface()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        I c = new C1();
                        c.Method();
                    }
                }
                public interface I { void Method(); }
                public class C1 : I {
                    public void Method() {
                        throw new InvalidOperationException();
                        I c = new C1();
                        c.Method();
                    }
                }
                public class C2 : I {
                    public void Method() {
                        throw new NotSupportedException();
                        I c = new C2();
                        c.Method();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateException(2, "NotSupportedException"),
            CodeHelper.CreateMethodExceptions(exceptions: ["System.InvalidOperationException", "System.NotSupportedException"]));
    }
}
