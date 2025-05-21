using JsonParser.Core.Visitors;

namespace JsonParser.Core;

public abstract record Element
{
    public abstract void Accept(IVisitor visitor);
    public abstract T Accept<T>(IVisitor<T> visitor);
}
public record ConstantElement(Token Value) : Element
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}
public record ArrayElement(Token Start, Element[] Elements, Token End) : Element
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}
public record MemberElement(Token<string> Name, Element Value) : Element
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}
public record ObjectElement(Token Start, MemberElement[] Memebers, Token End) : Element
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}