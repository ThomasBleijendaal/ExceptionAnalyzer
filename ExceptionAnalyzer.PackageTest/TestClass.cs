namespace ExceptionAnalyzer.PackageTest;

internal class TestClass
{
    private readonly ISomething _something;

    public TestClass(
        ISomething something)
    {
        _something = something;
    }

    [AddExceptions]
    public void TestMethod()
    {
        var x = "123";

        _something.Method();

        if (x == "123")
        {
            throw new NotSupportedException("Borked");
        }
        else
        {
            throw new InvalidCastException("Borky", new Exception());
        }
    }
}

internal interface ISomething
{
    void Method();
}

internal interface ISomethingOther
{
    void Method();
}

internal class ThisIsSomething1 : ISomething
{
    public void Method()
    {
        throw new NotImplementedException("1");
    }
}

internal class ThisIsSomething2 : ISomething
{
    private readonly ISomethingOther _something;

    public ThisIsSomething2(
        ISomethingOther something)
    {
        _something = something;
    }

    public void Method()
    {
        _something.Method();
        throw new NotImplementedException("2");
    }
}

internal class ThisIsSomethingElse : ISomethingOther
{
    public void Method()
    {
        throw new NotImplementedException("3");
    }
}
