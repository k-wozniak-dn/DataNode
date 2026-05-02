using System.Linq.Expressions;

namespace DataNode.Core;

public record Attribute(string Name, DnValue Value, IItem? Parent = null)
{
    #region Properties
    public string Name { get; } = Validator.ValidateName(Name);
    public DnValue Value { get; } = Validator.ValidateValue(Value);
    public IItem? Parent { get; set; } = Parent;
    public bool IsSystemAttribute
    {
        get
        {
            return Name.StartsWith(System.SysAttributeNamePrefix);
        }
    }
    public Attribute TakeOwnership(IItem parent)
    {
        return (Parent != parent) ? Copy(parent: parent) : this;
    }

    #endregion

    #region Convertion
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
        if (objectValue is string)
        {
            return (StringValue)objectValue;
        }
        else if (objectValue is int)
        {
            return (IntegerValue)objectValue;
        }
        else if (objectValue is decimal)
        {
            return (DecimalValue)objectValue;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported value type.");
        }
    }
    public string GetString()
    {
        return Value is StringValue stringValue ? stringValue.Value : 
            throw new InvalidOperationException($"Attribute '{Name}' does not contain a string value.");
    }
    public int GetInteger()
    {
        return Value is IntegerValue integerValue ? integerValue.Value : 
            throw new InvalidOperationException($"Attribute '{Name}' does not contain an integer value.");
    }
    public decimal GetDecimal()
    {
        return Value is DecimalValue decimalValue ? decimalValue.Value : 
            throw new InvalidOperationException($"Attribute '{Name}' does not contain a decimal value.");
    }
    
    #endregion

    #region Copy
    public static Attribute Copy(Attribute attribute, string? name = null, DnValue? value = null, IItem? parent = null)
    {
        return new Attribute(name ?? attribute.Name, value ?? attribute.Value, parent ?? attribute.Parent);
    }
    public Attribute Copy(string? name = null, DnValue? value = null, IItem? parent = null)
    {
        return Copy(this, name, value, parent);
    }
    
    #endregion

}