namespace JsonParser.Core.Visitors;

public class PrinterVisitor(TextWriter writer) : IVisitor
{
    private const string Padding = "\t";
    private readonly TextWriter _writer = writer;
    private string _identation = "";
    private void StartLine()
    {
        _writer.WriteLine();
        _writer.Write(_identation);
    }
    private void Push()
    {
        _identation += Padding;
    }
    private void Pop()
    {
        _identation = _identation[..^Padding.Length];
    }
    private void PrintText(string s)
    {
        s = s
            .Replace("\"", "\\\"")
            .Replace("\\", @"\")
            .Replace("/", @"\/")
            .Replace("\b", @"\b")
            .Replace("\f", @"f")
            .Replace("\n", @"\n")
            .Replace("\r", @"\r")
            .Replace("\t", @"\t");

        writer.Write('"');
        _writer.Write(s);
        _writer.Write('"');
    }
    public void Visit(ConstantElement e)
    {
        switch (e.Value)
        {
            case Token<double> td:
                _writer.Write(td.Value);
                break;
            case Token<bool> td:
                _writer.Write(td.Value.ToString().ToLowerInvariant());
                break;
            case Token<string> td:
                PrintText(td.Value);
                break;
            case { Type: TokenType.Nil }:
                _writer.Write("null");
                break;
        }
    }

    public void Visit(ArrayElement e)
    {
        _writer.Write('[');
        if (e.Elements.Length > 0)
        {
            bool isFirst = true;
            Push();
            foreach (var element in e.Elements)
            {
                if (!isFirst) { _writer.Write(','); }
                StartLine();
                element.Accept(this);
                isFirst = false;
            }
            Pop();
            StartLine();
        }
        _writer.Write(']');
    }

    public void Visit(MemberElement e)
    {
        PrintText(e.Name.Value);
        _writer.Write(": ");
        e.Value.Accept(this);
    }

    public void Visit(ObjectElement e)
    {
        _writer.Write('{');
        if (e.Memebers.Length > 0)
        {
            bool isFirst = true;
            Push();

            foreach (var m in e.Memebers)
            {
                if (!isFirst) { _writer.Write(','); }
                StartLine();
                m.Accept(this);
                isFirst = false;
            }
            Pop();
            StartLine();
        }
        _writer.Write('}');
    }
}