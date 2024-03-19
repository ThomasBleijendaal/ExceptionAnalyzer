using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Faultr.Tests.Helpers;

public static class GeneratorTestHelper
{
    private static (ImmutableArray<Diagnostic>, string[]) GetGeneratedOutput(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat(new[] { MetadataReference.CreateFromFile(typeof(FaultrGenerator).Assembly.Location) });
        var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new FaultrGenerator();

        var driver = CSharpGeneratorDriver.Create(ImmutableArray.Create<ISourceGenerator>(generator));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics,
            trees.Count != originalTreeCount
                ? trees.Skip(1).Select(x => x.ToString()).ToArray()
                : new string[0]);
    }

    public static void TestGeneratedCode(string sourceText, params string[] expectedOutputSourceTexts)
    {
        var (diagnostics, output) = GetGeneratedOutput(sourceText);

        foreach (var file in output)
        {
            Console.WriteLine(file);
            Console.WriteLine("-----------");
        }

        Assert.AreEqual(0, diagnostics.Length, string.Join(", ", diagnostics.Select(x => x.GetMessage())));

        expectedOutputSourceTexts = expectedOutputSourceTexts.ToArray();

        Assert.AreEqual(expectedOutputSourceTexts.Length, output.Length, $"Expected output files count miss-match");

        for (var i = 0; i < expectedOutputSourceTexts.Length; i++)
        {
            Assert.AreEqual(expectedOutputSourceTexts[i], output.ElementAtOrDefault(i) ?? "", $"Error in file index: {i}");
        }
    }

    public static void TestReportedDiagnostics(string sourceText, params string[] expectedDiagnosticErrors)
    {
        var (diagnostics, output) = GetGeneratedOutput(sourceText);

        var errorCodes = diagnostics.Select(x => x.Id).ToArray();

        Assert.AreEqual(expectedDiagnosticErrors.Length, diagnostics.Length, $"Found messages: {string.Join(", ", errorCodes)}.");

        foreach (var diagnostic in expectedDiagnosticErrors)
        {
            Assert.Contains(diagnostic, errorCodes);
        }
    }

    public static void TestReportedDiagnosticLocation(string sourceText, string errorCode, string locationText)
    {
        var (diagnostics, output) = GetGeneratedOutput(sourceText);

        diagnostics.Should().Contain(d =>
            d.Id == errorCode &&
            d.Location.SourceTree.ToString()
                .Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length) == locationText);
    }
}
