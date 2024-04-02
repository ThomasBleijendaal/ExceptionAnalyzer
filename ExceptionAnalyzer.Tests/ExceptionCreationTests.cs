using ExceptionAnalyzer.Tests.Helpers;
using NUnit.Framework;

namespace ExceptionAnalyzer.Tests;

public class ExceptionCreationTests
{
    [Test]
    public void ThrowMultiple()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        throw new Exception();
                        throw new Exception("Message");
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, ["return new Exception();"]),
            CodeHelper.CreateExceptionCreation(2, ["return new Exception(\"Message\");"]),
            CodeHelper.CreateMethodExceptions(exceptions: new[] { "System.Exception", "System.Exception" }));
    }

    [Test]
    public void ThrowWithConstants()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class Constant { public const string Message = "Message" }
                public class B {
                    [AddExceptions]
                    public void Method() {
                        throw new Exception(Constant.Message);
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, ["return new Exception(Constant.Message);"]),
            CodeHelper.CreateMethodExceptions(exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowWithNestedNamespace()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                namespace Q {
                    public class Constant { public const string Message = "Message" }
                    public class B {
                        [AddExceptions]
                        public void Method() {
                            throw new Exception(Constant.Message);
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, ["return new Exception(Constant.Message);"], "A.Q"),
            CodeHelper.CreateMethodExceptions("A.Q.B", exceptions: "System.Exception"));
    }

    [Test]
    public void ThrowWithVariable()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        try {
                            throw new Exception(Constant.Message);
                        }
                        catch (Exception ex) {
                            throw new InvalidOperationException("Bork", ex);
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, [
                "System.Exception ex = default!;",
                "return new InvalidOperationException(\"Bork\", ex);"
            ]),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowWithStructVariable()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        var i = 123;
                        throw new CustomException(i);
                    }
                }

                public class CustomException(int i) : Exception { }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, [
                "int i = default!;",
                "return new CustomException(i);"
            ]),
            CodeHelper.CreateMethodExceptions(exceptions: "A.CustomException"));
    }

    [Test]
    public void ThrowWithException()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        try {
                        }
                        catch (Exception ex) {
                            throw new InvalidOperationException("Bork", ex);
                        }
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, [
                "System.Exception ex = default!;",
                "return new InvalidOperationException(\"Bork\", ex);"
            ]),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }

    [Test]
    public void ThrowWithFormatExpression()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using System;
            using ExceptionAnalyzer;

            namespace A {
                public class B {
                    [AddExceptions]
                    public void Method() {
                        var i = 123;
                        throw new InvalidOperationException($"Borked {i}");
                    }
                }
            }
            """,
            CodeHelper.CreateExceptionCreation(1, [
                "int i = default!;",
                "return new InvalidOperationException($\"Borked {i}\");"
            ]),
            CodeHelper.CreateMethodExceptions(exceptions: "System.InvalidOperationException"));
    }
}

