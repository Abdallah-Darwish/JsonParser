using JsonParser.Core;
using JsonParser.Core.Visitors;

namespace JsonParser.Tests;

public static class Utility
{
    public static Tokenizer GetTokenizer(string js) => new(new StringReader(js));
    public static Token NextToke(string js)
    {
        using var tok = GetTokenizer(js);
        return tok.AdvanceAndGetNext();
    }
    public static Token<T> NextToke<T>(string js) => NextToke(js) as Token<T>;
    public static Parser GetParser(string js) => new(GetTokenizer(js));
    public static Element Parse(string js)
    {
        using var parser = GetParser(js);
        return parser.Parse();
    }
    public static string Print(string js)
    {
        using StringWriter sw = new();
        PrinterVisitor printer = new(sw);
        Parse(js).Accept(printer);
        return sw.ToString();
    }
}