namespace JsonParser.Core;

public static class Utility
{
    public static bool IsWhitespace(char c) => c is '\u0020' or '\u000A' or '\u000D' or '\u0009';
}