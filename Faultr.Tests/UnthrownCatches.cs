using Faultr.Tests.Helpers;
using NUnit.Framework;

namespace Faultr.Tests;

// TODO: this should become a feature flag
public class UnthrownCatches
{
    [Test]
    public void ThrowFromCatchWithUnusedCatchException()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        try {
                            throw new InvalidOperationException();
                        }
                        catch (NotSupportedException ex) {
                            throw new NullReferenceException();
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "NullReferenceException"),
            CodeHelper.CreateException(2, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.NullReferenceException", "System.InvalidOperationException" }));
    }

    [Test]
    public void ThrowFromCatchWithUsedAndUnusedCatchException()
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
                        catch (NotSupportedException ex) {
                            throw new NullReferenceException();
                        }
                        catch (Exception ex) {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "NullReferenceException"),
            CodeHelper.CreateException(2, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.NullReferenceException", "System.InvalidOperationException" }));
    }

    [Test]
    public void ThrowFromCatchWithUsedAndUnusedShortCatchException()
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
                        catch (NotSupportedException) {
                            throw new NullReferenceException();
                        }
                        catch (Exception) {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "NullReferenceException"),
            CodeHelper.CreateException(2, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.NullReferenceException", "System.InvalidOperationException" }));
    }

    [Test]
    public void ThrowFromCatchWithUnusedCatchRethrowException()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using Faultr;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        try {
                            throw new InvalidOperationException();
                        }
                        catch (NotSupportedException ex) {
                            throw;
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { (false, "System.NotSupportedException"), (true, "System.InvalidOperationException") }));
    }

    [Test]
    public void ThrowFromNestedCatchWithImpossibleException()
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
                                throw new InvalidOperationException();
                            }
                            catch (NotSupportedException ex) {
                                throw new NullReferenceException();
                            }
                        }
                        catch (Exception ex) {
                            throw;
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateException(1, "NullReferenceException"),
            CodeHelper.CreateException(2, "InvalidOperationException"),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.NullReferenceException", "System.InvalidOperationException" }));
    }
}
