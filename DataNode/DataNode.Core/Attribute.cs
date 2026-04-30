namespace DataNode.Core;

public record Attribute(string Name, DnValue Value, IItem? Parent = null)
{
    public string Name { get; } = ValidateName(Name);
    public DnValue Value { get; } = ValidateValue(Value);
    public IItem? Parent { get; set; } = Parent;
    public static Attribute Copy(Attribute attribute, string? name = null, DnValue? value = null, IItem? parent = null)
    {
        return new Attribute(name ?? attribute.Name, value ?? attribute.Value, parent ?? attribute.Parent);
    }
    public Attribute Copy(string? name = null, DnValue? value = null, IItem? parent = null)
    {
        return Copy(this, name, value, parent);
    }
    public Attribute TakeOwnership(IItem parent)
    {
        return (Parent != parent) ? Copy(parent: parent) : this;
    }
    public bool IsSystemAttribute
    {
        get
        {
            return Name.StartsWith(System.SysAttributeNamePrefix);
        }
    }
    
    #region Validate
    public static string ValidateName(string name)
    {
        name = name.Trim().ToUpper();
        if (name.Length > System.AttributeNameLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {System.AttributeNameLengthLimit} characters.");
        }
        return name;
    }
    public static string ValidateStringValue(string stringValue)
    {
        if (stringValue.Length > System.StringValueLengthLimit)
        {
            throw new ArgumentException($"String value length exceeds the limit of {System.StringValueLengthLimit} characters.");
        }
        return stringValue;
    }
    public static DnValue ValidateValue(DnValue value)
    {
        if (value is StringValue stringValue)
        {
            ValidateStringValue(stringValue.Value);
        }
        return value;
    }
    
    #endregion

    #region GetValue
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
}