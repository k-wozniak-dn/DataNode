namespace DataNode.Core;

public class Item(Dictionary<string, DnValue> attributes, string key, IParentDataNode? parent = null)
{
    #region Properties
    public Dictionary<string, DnValue> Attributes { get; } = attributes;
    public string Key { get; } = ValidateKey(key);
    public IParentDataNode? Parent { get; set; } = parent;
    public int? Index
    {
        get
        {
            return Parent?.GetIndex(Key);
        }
    }

    #endregion

    public Item(string key, IParentDataNode? parent = null) : this([], key, parent)
    {
    }

    #region Validate
    private static string ValidateKey(string key)
    {
        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        return key.ToUpper();
    }
    
    private static string ValidateAttributeName(string attributeName)
    {
        if (attributeName.Length > System.AttributeNameLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {System.AttributeNameLengthLimit} characters.");
        }
        return attributeName.ToUpper();
    }

    private static string ValidateStringValue(string stringValue)
    {
        if (stringValue.Length > System.StringValueLengthLimit)
        {
            throw new ArgumentException($"String value length exceeds the limit of {System.StringValueLengthLimit} characters.");
        }
        return stringValue;
    }

    private void ValidateAttributeCount(string attributeName)
    {
        if (Count() >= System.AttributesCountLimit && !Attributes.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
    }

    private void ValidateSetExisting(string attributeName, bool existingOnly)
    {
        if (existingOnly && !Contains(attributeName))
        {
            throw new InvalidOperationException($"Attribute '{attributeName}' does not exist. Use Add to add.");
        }
    }

    #endregion

    #region Get
    public DnValue? Get(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        if (Attributes.TryGetValue(attributeName, out DnValue? dnValue))
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

    public IEnumerable<KeyValuePair<string, DnValue>> GetAll()
    {
        return [.. Attributes.Select(kvp => new KeyValuePair<string, DnValue>(kvp.Key, kvp.Value))];
    }
    
    #endregion

    #region Set
    public Item Set(string attributeName, string value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        value = ValidateStringValue(value);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        Attributes[attributeName] = value;
        return this;
    }

    public Item Set(string attributeName, int value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        Attributes[attributeName] = value;
        return this;
    }

    public Item Set(string attributeName, decimal value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        Attributes[attributeName] = value;
        return this;
    }

    public Item Set(string attributeName, DnValue value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        Attributes[attributeName] = value;
        return this;
    }

    public Item SetAll(Dictionary<string, DnValue> attributes, bool existingOnly = false)
    {
        foreach (var kvp in attributes)
        {
            Set(kvp.Key, kvp.Value, existingOnly);
        }
        return this;
    }

    public Item SetAll(Dictionary<string, object> values, bool existingOnly = false)
    {
        foreach (var kvp in values)
        {
            if (kvp.Value is string stringValue)
            {
                Set(kvp.Key, stringValue, existingOnly);
            }
            else if (kvp.Value is int intValue)
            {
                Set(kvp.Key, intValue, existingOnly);
            }
            else if (kvp.Value is decimal decimalValue)
            {
                Set(kvp.Key, decimalValue, existingOnly);
            }
            else
            {
                throw new ArgumentException($"Unsupported value type for key '{kvp.Key}'.");
            }
        }
        return this;
    }

    #endregion

    #region Add
    public Item Add(string attributeName, string value)
    {
        attributeName = ValidateAttributeName(attributeName);
        value = ValidateStringValue(value);
        ValidateAttributeCount(attributeName);

        Attributes.Add(attributeName, value);
        return this;
    }

    public Item Add(string attributeName, int value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        Attributes.Add(attributeName, value);
        return this;
    }

    public Item Add(string attributeName, decimal value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        Attributes.Add(attributeName, value);
        return this;
    }

    public Item Add(string attributeName, DnValue value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        Attributes.Add(attributeName, value);
        return this;
    }

    public Item AddAll(Dictionary<string, DnValue> attributes)
    {
        foreach (var kvp in attributes)
        {
            Add(kvp.Key, kvp.Value);
        }
        return this;
    }

    public Item AddAll(Dictionary<string, object> attributes)
    {
        foreach (var kvp in attributes)
        {
            if (kvp.Value is string stringValue)
            {
                Add(kvp.Key, stringValue);
            }
            else if (kvp.Value is int intValue)
            {
                Add(kvp.Key, intValue);
            }
            else if (kvp.Value is decimal decimalValue)
            {
                Add(kvp.Key, decimalValue);
            }
            else
            {
                throw new ArgumentException($"Unsupported value type for key '{kvp.Key}'.");
            }
        }
        return this;
    }

    #endregion

    public Item Remove(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        Attributes.Remove(attributeName);
        return this;
    }

    public bool Contains(string attributeName)
    {
        attributeName = ValidateAttributeName(attributeName);
        return Attributes.ContainsKey(attributeName);
    }

    public Item Copy(string key, IParentDataNode? parent)
    {
        return new Item(new Dictionary<string, DnValue>(Attributes), key, parent);
    }

    public int Count(bool includeSystemAttributes = false) { 
        return includeSystemAttributes ? 
            Attributes.Count : 
            Attributes.Count(kvp => !kvp.Key.StartsWith(System.SysAttributeNamePrefix));
        }
}