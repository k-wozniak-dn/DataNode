namespace DataNode.Core;

public static class System
{
    public const short AttributeNameLengthLimit = 64;
    public const short AttributesCountLimit = 16;
    public const short StringValueLengthLimit = 256;
    public const short KeyLengthLimit = 64;
    public const short ItemsCountLimit = 64;
    public const string SysKeyPrefix = "*";
    public const string SysAttributeNamePrefix = "*"; 
}

public static class SystemAttributes
{
    public const string Indexed = $"{System.SysAttributeNamePrefix}IDX";
    public static readonly HashSet<string> All = [Indexed];
}

public static class SystemKeys
{
    public const string Header = $"{System.SysKeyPrefix}ID";
    public static readonly HashSet<string> All = [Header];
}

public static class Validator
{
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
    public static string ValidateKey(string key)
    {
        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        return key.ToUpper();
    }
    
}