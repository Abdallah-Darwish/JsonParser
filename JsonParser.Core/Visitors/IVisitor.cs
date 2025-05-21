namespace JsonParser.Core.Visitors;


public interface IVisitor
{
    void Visit(ConstantElement e);
    void Visit(ArrayElement e);
    void Visit(MemberElement e);
    void Visit(ObjectElement e);
}
public interface IVisitor<T>
{
    T Visit(ConstantElement e);
    T Visit(ArrayElement e);
    T Visit(MemberElement e);
    T Visit(ObjectElement e);
}