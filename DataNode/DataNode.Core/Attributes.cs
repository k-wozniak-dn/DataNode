namespace DataNode.Core;

public record Attributes(Dictionary<string, DnValue> DnValueDictionary, IParentDataNode? InitParent = null, string? InitKey = null)
{
    public string? Key { get; set; } = InitKey;

    public IParentDataNode? Parent { get; set; } = InitParent;

    private static string ValidateAttributeName(string attributeName)
    {
        if (attributeName.Length > System.AttributeNameLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {System.AttributeNameLengthLimit} characters.");
        }
        return attributeName.ToUpper();
    }
    
    public static Attributes Create(IParentDataNode? parent = null, string? key = null) => new([], parent, key);

    public DnValue? Get(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        if (DnValueDictionary.TryGetValue(attributeName, out DnValue? dnValue))
        {
            return dnValue;
        }
        else
        {
            return null;
        }
    }

    public string? GetString(string attributeName)
    {
        var value = Get(attributeName);
        return value is StringValue stringValue ? stringValue.Value : null;
    }

    public int? GetInteger(string attributeName)
    {
        var value = Get(attributeName);
        return value is IntegerValue integerValue ? integerValue.Value : null;
    }

    public decimal? GetDecimal(string attributeName)
    {
        var value = Get(attributeName);
        return value is DecimalValue decimalValue ? decimalValue.Value : null;
    }

    public void Set(string attributeName, string value)
    {
        attributeName = ValidateAttributeName(attributeName);
        if ( value.Length > System.StringValueLengthLimit)
        {
            throw new ArgumentException($"String value length exceeds the limit of {System.StringValueLengthLimit} characters.");
        }
        if (DnValueDictionary.Count >= System.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Set(string attributeName, int value)
    {
        attributeName = ValidateAttributeName(attributeName);
        if (DnValueDictionary.Count >= System.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Set(string attributeName, decimal value)
    {
        attributeName = ValidateAttributeName(attributeName);
        if (DnValueDictionary.Count >= System.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Remove(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        DnValueDictionary.Remove(attributeName);
    }

    public bool Contains(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        return DnValueDictionary.ContainsKey(attributeName);
    }

    public IEnumerable<KeyValuePair<string, DnValue>> All()
    {
        return [.. DnValueDictionary.Select(kvp => new KeyValuePair<string, DnValue>(kvp.Key, kvp.Value))];
    }

    public Attributes Copy(IParentDataNode? parent, string? key = null)
    {
        return new Attributes(new Dictionary<string, DnValue>(DnValueDictionary), parent, key);
    }

    public int Count { get { return DnValueDictionary.Count; } }

}