namespace JsonParser.Core;

public class ParserException(Token token, string message) : Exception(message)
{
    public Token Token { get; } = token;
}

public class Parser(Tokenizer source) : IDisposable
{
    private bool _is_disposed = false, _is_exhausted = false;
    private void GuradDisposed()
    {
        if (!_is_disposed)
        {
            return;
        }
        throw new ObjectDisposedException(null);
    }
    readonly Tokenizer _source = source;
    public bool IsExhausted => _is_exhausted;
    public Element Parse()
    {
        GuradDisposed();
        if (_is_exhausted) { throw new InvalidOperationException("Parser source is already exhausted"); }
        _source.Advance();
        var ret = ParseElement();
        if (_source.Current.Type != TokenType.Eof)
        {
            throw new ParserException(_source.Current, $"Source has more than one root element, second one at ({_source.Current.Line}, {_source.Current.Column})");
        }
        _is_exhausted = true;
        return ret;
    }

    private Element ParseElement()
    {
        IgnoreWhitespace();
        var val = ParseValue();
        IgnoreWhitespace();
        return val;
    }
    private Element ParseValue()
    {
        if (_source.Current.Type is TokenType.Boolean or TokenType.Nil or TokenType.Text or TokenType.Number)
        {
            return new ConstantElement(_source.AdvanceAndGetCurrent());
        }
        if (_source.Current.Type == TokenType.LeftBracket) { return ParseArray(); }
        return ParseObject();
    }
    private ArrayElement ParseArray()
    {
        List<Element> elements = [];
        var start = _source.AdvanceAndGetCurrent(TokenType.LeftBracket);
        IgnoreWhitespace();
        if (_source.Current.Type != TokenType.RightBracket)
        {
            elements.Add(ParseElement());
        }
        while (_source.Current.Type == TokenType.Comma)
        {
            _source.Advance();
            elements.Add(ParseElement());
        }
        var end = _source.AdvanceAndGetCurrent(TokenType.RightBracket);
        return new ArrayElement(start, [.. elements], end);
    }
    private MemberElement ParseMemeber()
    {
        IgnoreWhitespace();
        var name = _source.AdvanceAndGetCurrent(TokenType.Text) as Token<string>;
        IgnoreWhitespace();
        _source.AdvanceAndGetCurrent(TokenType.Colon);
        var val = ParseElement();
        return new(name, val);
    }
    private ObjectElement ParseObject()
    {
        HashSet<string> seenNames = [];
        List<MemberElement> members = [];
        void ParseMember()
        {
            var member = ParseMemeber();
            if (!seenNames.Add(member.Name.Value))
            {
                throw new ParserException(member.Name, $"Duplicate name {member.Name.Value} at ({member.Name.Line}, {member.Name.Column})");
            }
            members.Add(member);
        }
        var start = _source.AdvanceAndGetCurrent(TokenType.LeftBrace);
        IgnoreWhitespace();
        if (_source.Current.Type != TokenType.RightBrace)
        {
            ParseMember();
        }
        while (_source.Current.Type == TokenType.Comma)
        {
            _source.Advance();
            ParseMember();
        }
        var end = _source.AdvanceAndGetCurrent(TokenType.RightBrace);
        return new ObjectElement(start, [.. members], end);
    }
    private void IgnoreWhitespace()
    {
        if (!_source.IsExhausted && _source.Current.Type == TokenType.Whitespace)
        {
            _source.Advance();
        }
    }

    public void Dispose()
    {
        if (_is_disposed) { return; }
        _is_disposed = true;
        ((IDisposable)_source).Dispose();
    }
}