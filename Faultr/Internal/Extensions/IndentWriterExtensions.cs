using System.CodeDom.Compiler;

namespace Faultr.Internal.Extensions;

public static class IndentWriterExtensions
{
    public static IDisposable NoIndent(this IndentedTextWriter indentWriter) => new IndentNothing();
    public static IDisposable Indent(this IndentedTextWriter indentWriter) => new IndentDisposable(indentWriter);
    public static IDisposable Braces(this IndentedTextWriter indentWriter) => indentWriter.Braces("");
    public static IDisposable BracesWithSemiColon(this IndentedTextWriter indentWriter) => indentWriter.Braces(";");
    public static IDisposable BracesWithComma(this IndentedTextWriter indentWriter) => indentWriter.Braces(",");
    public static IDisposable Braces(this IndentedTextWriter indentWriter, string extra) => new IndentDisposable(indentWriter, "{", $"}}{extra}");

    private class IndentDisposable : IDisposable
    {
        private readonly IndentedTextWriter _indentWriter;
        private readonly string? _after;

        public IndentDisposable(IndentedTextWriter indentWriter, string? before = null, string? after = null)
        {
            _indentWriter = indentWriter;
            _after = after;
            if (before != null)
            {
                _indentWriter.WriteLine(before);
            }
            _indentWriter.Indent++;
        }

        public void Dispose()
        {
            _indentWriter.Indent--;
            if (_after != null)
            {
                _indentWriter.WriteLine(_after);
            }
        }
    }

    private class IndentNothing : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
