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
    public static object ToObjectValue(DnValue value)
    {
        if (value is StringValue stringValue)
        {
            return stringValue.Value;
        }
        else if (value is IntegerValue integerValue)
        {
            return integerValue.Value;
        }
        else if (value is DecimalValue decimalValue)
        {
            return decimalValue.Value;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported value type.");
        }
    }
    public static DnValue FromObjectValue(object objectValue)
    {
        if (objectValue is string vs)
        {
            return new StringValue(vs);
        }
        else if (objectValue is int vInt)
        {
            return new IntegerValue(vInt);
        }
        else if (objectValue is decimal vDec)
        {
            return new DecimalValue(vDec);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported value type.");
        }
    }
}

public record StringValue(string Value) : DnValue;
public record IntegerValue(int Value) : DnValue;
public record DecimalValue(decimal Value) : DnValue;