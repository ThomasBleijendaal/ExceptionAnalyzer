using Faultr.Tests.Helpers;
using NUnit.Framework;

namespace Faultr.Tests;

public class MethodCallThrows
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
                        SomeMethod();
                    }

                    private void SomeMethod() {
                        throw new Exception();
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "Exception"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowOverload()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

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
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowFromNestedMethod()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

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
            }
            """,
            CodeHelper.CreateException(1, "Exception"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowFromCatch()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

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
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowFromNestedCatch()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        try {
                            try {
                                SomeMethod();
                            }
                            catch (InvalidOperationException ex) {
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
            }
            """,
            CodeHelper.CreateException(1, "NullReferenceException"),
            CodeHelper.CreateMethodExceptions(exceptions: "System.NullReferenceException"));
    }
}
