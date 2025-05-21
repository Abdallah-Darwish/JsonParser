namespace JsonParser.Core;

public enum TokenType
{
    Text,
    Boolean,
    Nil,
    RightBrace,
    LeftBrace,
    RightBracket,
    LeftBracket,
    Comma,
    Colon,
    Number,
    Eof,
    Whitespace
}
public record class Token(int Line, int Column, TokenType Type);
public record class Token<T>(int Line, int Column, TokenType Type, T Value) : Token(Line, Column, Type);