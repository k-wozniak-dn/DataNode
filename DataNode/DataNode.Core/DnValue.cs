namespace DataNode.Core;

public abstract record DnValue
{
    public static implicit operator DnValue(string v)
    {
        return new StringValue(v);
    }

    public static implicit operator DnValue(int v)
    {
        return new IntegerValue(v);
    }

    public static implicit operator DnValue(decimal v)
    {
        return new DecimalValue(v);
    }
}

public record StringValue(string Value) : DnValue;
public record IntegerValue(int Value) : DnValue;
public record DecimalValue(decimal Value) : DnValue;