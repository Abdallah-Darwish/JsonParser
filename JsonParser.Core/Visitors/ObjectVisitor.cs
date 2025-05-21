namespace JsonParser.Core.Visitors;

public class ObjectVisitor : IVisitor<object?>
{
    public object? Visit(ConstantElement e) => e.Value switch
    {
        Token<double> td => td.Value,
        Token<bool> td => td.Value,
        Token<string> td => td.Value,
        _ => null,
    };

    public object? Visit(ArrayElement e) => e.Elements.Select(e => e.Accept(this)).ToArray();

    public object Visit(MemberElement e)
    {
        return new KeyValuePair<string, object?>(e.Name.Value, e.Value.Accept(this));
    }

    public object Visit(ObjectElement e) => e.Memebers.Select(i => i.Accept(this)).OfType<KeyValuePair<string, object?>>().ToDictionary();
}