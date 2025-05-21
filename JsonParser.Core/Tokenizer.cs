using System.Text;

namespace JsonParser.Core;

public class TokenizerException(int line, int column, char value, string message) : Exception(message)
{
    public int Line { get; } = line;
    public int Column { get; } = column;
    public char Value { get; } = value;
}
public class Tokenizer(StringReader source) : IDisposable
{
    #region Source Management
    private void GuradDisposed()
    {
        if (!_is_disposed)
        {
            return;
        }
        throw new ObjectDisposedException(null);
    }
    bool _is_disposed = false;
    private const char Eof = '\0';
    private readonly TextReader _source = source;
    int _line = 1, _column = 1;

    private char Peek()
    {
        int c = _source.Peek();
        if (c == -1) { return Eof; }
        return (char)c;
    }

    private bool IsSourceExhausted
    {
        get
        {
            return Peek() == Eof;
        }
    }

    private char Consume()
    {
        if (Peek() == '\n')
        {
            _line++;
            _column = 1;
        }
        else { _column++; }
        if (IsSourceExhausted) { return Eof; }
        return (char)_source.Read();
    }
    #endregion

    private Token ReadKeyword()
    {
        int line = _line, col = _column;
        StringBuilder sb = new();
        while (sb.Length < 4)
        {
            if (IsSourceExhausted)
            {
                throw ThrowException($"Encountered an unexpected value '{sb}' while trying to parse a keyword starting at ({line}, {col}), Expected one of (true, false, null)", line, col);
            }
            sb.Append(Consume());
        }
        var sbStr = sb.ToString();
        if (sbStr == "true") { return new Token<bool>(line, col, TokenType.Boolean, true); }
        if (sbStr == "null") { return new Token(line, col, TokenType.Nil); }
        if (sbStr == "fals" && Consume() == 'e') { return new Token<bool>(line, col, TokenType.Boolean, false); }

        throw ThrowException($"Encountered an unexpected value '{sb}' while trying to parse a keyword starting at ({line}, {col}), Expected one of (true, false, null)", line, col);
    }
    #region Text
    private Token<string> ReadString()
    {
        int line = _line, col = _column;
        char c = Peek();
        if (c != '\"')
        {
            throw ThrowException($"Encountered an unexpected value '{c}' while trying to parse a string starting at ({line}, {col}), Expected '\"'");
        }
        StringBuilder sb = new();
        Consume();
        while (!IsSourceExhausted)
        {
            c = Peek();
            if (c == '\"')
            {
                Consume();
                return new(line, col, TokenType.Text, sb.ToString());
            }
            if (c == '\\')
            {
                sb.Append(ReadEscape());
                continue;
            }
            if (c < '\u0020')
            {
                throw ThrowException($"Encountered an unexpected value '{c}' while trying to parse a string starting at ({line}, {col})");
            }
            sb.Append(c);
            Consume();
        }
        throw ThrowException($"Unterminated string {sb} starting at ({line}, {col})");
    }
    private char ReadEscape()
    {
        int line = _line, col = _column;
        char c = Peek();
        if (c != '\\')
        {
            throw ThrowException($"Encountered an unexpected value '{c}' while trying to parse an escape sequence starting at ({line}, {col}), Expected '\\'");
        }
        Consume();
        c = Peek();
        switch (c)
        {
            case '\"':
                Consume();
                return '\"';
            case '\\':
                Consume();
                return '\\';
            case '/':
                Consume();
                return '/';
            case 'b':
                Consume();
                return '\b';
            case 'f':
                Consume();
                return '\f';
            case 'n':
                Consume();
                return '\n';
            case 'r':
                Consume();
                return '\r';
            case 't':
                Consume();
                return '\t';
            case 'u':
                Consume();
                StringBuilder sb = new();
                for (int i = 0; i < 4; i++)
                {
                    if (!ReadHex(sb))
                    {
                        throw ThrowException($"Encountered an unexpected value '{Peek()}' at ({_line}, {_column}) while trying to parse a unicode escape sequence starting at ({line}, {col})");
                    }
                }
                return (char)int.Parse(sb.ToString(), System.Globalization.NumberStyles.HexNumber);
            default:
                throw ThrowException($"Encountered an unexpected value '{Peek()}' at ({_line}, {_column}) while trying to parse an escape sequence starting at ({line}, {col}), Expected one of (\", \\, /, b, f, n, r, t, u)");
        }
    }
    private bool ReadHex(StringBuilder sb)
    {
        char c = Peek();
        if (c is >= 'a' and <= 'f' or >= 'A' and <= 'F')
        {
            sb.Append(Consume());
            return true;
        }
        return ReadDigit(sb);
    }
    #endregion
    #region Number
    private bool ReadDigit(StringBuilder sb, bool allowZero = true)
    {
        char c = Peek();
        if (c >= '1' && c <= '9' || (allowZero && c == '0'))
        {
            sb.Append(Consume());
            return true;
        }
        return false;
    }
    private bool ReadDigits(StringBuilder sb)
    {
        if (!ReadDigit(sb))
        {
            return false;
        }
        while (ReadDigit(sb)) { }
        return true;
    }
    private bool ReadUnsignedInteger(StringBuilder sb)
    {
        if (!ReadDigit(sb))
        {
            return false;
        }
        if (sb[^1] == '0') { return true; }
        ReadDigits(sb);
        return true;
    }
    private bool ReadSignedInteger(StringBuilder sb)
    {
        if (Peek() == '-')
        {
            sb.Append(Consume());
        }
        return ReadUnsignedInteger(sb);
    }
    private bool ReadFraction(StringBuilder sb)
    {
        if (Peek() == '.')
        {
            sb.Append(Consume());
            return ReadDigits(sb);
        }
        return true;
    }
    private bool ReadSign(StringBuilder sb)
    {
        if (Peek() is '-' or '+')
        {
            sb.Append(Consume());
        }
        return true;
    }
    private bool ReadExponent(StringBuilder sb)
    {
        if (Peek() is 'E' or 'e')
        {
            sb.Append(Consume());
            return ReadSign(sb) && ReadDigits(sb);
        }
        return true;
    }
    private Token<double> ReadNumber()
    {
        int line = _line, col = _column;
        StringBuilder sb = new();
        if (!ReadSignedInteger(sb) || !ReadFraction(sb) || !ReadExponent(sb))
        {
            throw ThrowException($"Encountered an unexpected value '{Peek()}' while parsing a number at ({_line}, {_column})");
        }
        return new Token<double>(line, col, TokenType.Number, double.Parse(sb.ToString()));
    }
    #endregion

    Token? _current = null;

    public Token Current
    {
        get
        {
            GuradDisposed();
            return _current ?? throw new InvalidOperationException("Please call Next first");
        }
    }

    public bool IsExhausted
    {
        get
        {
            GuradDisposed();
            return _current?.Type == TokenType.Eof;
        }
    }
    private TokenizerException ThrowException(string? message = null, int line = -1, int col = -1)
    {
        if (line == -1) { line = _line; }
        if (col == -1) { col = _column; }
        message ??= $"Unexpected value '{Peek()}' at ({line}, {col})";
        return new(line, col, Peek(), message);
    }

    private Token GetToken(TokenType type)
    {
        Token res = new(_line, _column, type);
        Consume();
        return res;
    }
    private Token? ReadWhitespace()
    {
        if (!Utility.IsWhitespace(Peek())) { return null; }
        int line = _line, col = _column;
        while (Utility.IsWhitespace(Peek())) { Consume(); }
        return new(line, col, TokenType.Whitespace);
    }

    private Token Tokenize()
    {
        if (IsExhausted)
        {
            throw new InvalidOperationException("Source is already exhausted");
        }
        var ws = ReadWhitespace();
        if (ws is not null) { return ws; }
        char c = Peek();
        switch (c)
        {
            case '{':
                return GetToken(TokenType.LeftBrace);
            case '}':
                return GetToken(TokenType.RightBrace);
            case '[':
                return GetToken(TokenType.LeftBracket);
            case ']':
                return GetToken(TokenType.RightBracket);
            case ':':
                return GetToken(TokenType.Colon);
            case ',':
                return GetToken(TokenType.Comma);
            case Eof:
                return GetToken(TokenType.Eof);
            default:
                break;
        }
        if (c is '-' or >= '0' and <= '9') { return ReadNumber(); }
        if (c == '"') { return ReadString(); }
        return ReadKeyword();
    }
    public void Advance()
    {
        GuradDisposed();
        _current = Tokenize();
    }
    public Token AdvanceAndGetNext()
    {
        Advance();
        return Current;
    }
    public Token AdvanceAndGetCurrent(TokenType? type = null)
    {
        GuradDisposed();
        if (type is not null && Current.Type != type)
        {
            throw new ParserException(Current, $"Expected a token of type {type} but got {Current}");
        }
        var cur = Current;
        Advance();
        return cur;
    }

    public void Dispose()
    {
        if (_is_disposed) { return; }
        _is_disposed = true;
        ((IDisposable)_source).Dispose();
    }
}