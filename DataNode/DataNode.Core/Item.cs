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
    public static string ValidateKey(string key)
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
    public Dictionary<string, DnValue> Get()
    {
        return Attributes.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }    
    
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

    #endregion

    #region Set
    public Item Set(string attributeName, string value, bool existingOnly = false, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        value = ValidateStringValue(value);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes[attributeName] = value;
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Set(string attributeName, int value, bool existingOnly = false, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes[attributeName] = value;
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Set(string attributeName, decimal value, bool existingOnly = false, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes[attributeName] = value;
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Set(string attributeName, DnValue value, bool existingOnly = false, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes[attributeName] = value;
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item SetAll(Dictionary<string, DnValue> attributes, bool existingOnly = false, bool skipHandlers = false )
    {
        foreach (var kvp in attributes)
        {
            Set(kvp.Key, kvp.Value, existingOnly, skipHandlers);
        }
        return this;
    }

    public Item SetAll(Dictionary<string, object> values, bool existingOnly = false, bool skipHandlers = false)
    {
        foreach (var kvp in values)
        {
            if (kvp.Value is string stringValue)
            {
                Set(kvp.Key, stringValue, existingOnly, skipHandlers);
            }
            else if (kvp.Value is int intValue)
            {
                Set(kvp.Key, intValue, existingOnly, skipHandlers);
            }
            else if (kvp.Value is decimal decimalValue)
            {
                Set(kvp.Key, decimalValue, existingOnly, skipHandlers);
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
    public Item Add(string attributeName, string value, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        value = ValidateStringValue(value);
        ValidateAttributeCount(attributeName);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes.Add(attributeName, value);
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Add(string attributeName, int value, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes.Add(attributeName, value);
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Add(string attributeName, decimal value, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes.Add(attributeName, value);
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item Add(string attributeName, DnValue value, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        if (!skipHandlers) { OnBeforeAttributeSet(attributeName); }
        Attributes.Add(attributeName, value);
        if (!skipHandlers) { OnAfterAttributeSet(attributeName); }

        return this;
    }

    public Item AddAll(Dictionary<string, DnValue> attributes, bool skipHandlers = false)
    {
        foreach (var kvp in attributes)
        {
            Add(kvp.Key, kvp.Value, skipHandlers);
        }
        return this;
    }

    public Item AddAll(Dictionary<string, object> attributes, bool skipHandlers = false)
    {
        foreach (var kvp in attributes)
        {
            if (kvp.Value is string stringValue)
            {
                Add(kvp.Key, stringValue, skipHandlers);
            }
            else if (kvp.Value is int intValue)
            {
                Add(kvp.Key, intValue, skipHandlers);
            }
            else if (kvp.Value is decimal decimalValue)
            {
                Add(kvp.Key, decimalValue, skipHandlers);
            }
            else
            {
                throw new ArgumentException($"Unsupported value type for key '{kvp.Key}'.");
            }
        }
        return this;
    }

    #endregion

    public Item Remove(string attributeName, bool skipHandlers = false)
    {
        attributeName = ValidateAttributeName(attributeName);

        if (!skipHandlers) { OnBeforeAttributeRemove(attributeName); }
        Attributes.Remove(attributeName);
        if (!skipHandlers) { OnAfterAttributeRemove(attributeName); }

        return this;
    }

    #region Handlers

    protected virtual void OnBeforeAttributeSet(string attributeName)
    {
        if (attributeName.StartsWith(System.SysAttributeNamePrefix))
        {
            switch (attributeName)
            {
                case SystemAttributes.Indexed:
                    if (Key.StartsWith(System.SysKeyPrefix))
                    {
                        throw new InvalidOperationException($"Attribute '{attributeName}' cannot be set on system keys.");
                    }
                break;
            }
        }
        else
        {
            switch (attributeName)
            {
            }
        }
    }

    protected virtual void OnAfterAttributeSet(string attributeName)
    {
        if (attributeName.StartsWith(System.SysAttributeNamePrefix))
        {
            switch (attributeName)
            {
                case SystemAttributes.Indexed:
                    Parent?.SetIndex(Key);
                    break;
            }
        }
        else
        {
            switch (attributeName)
            {
            }
        }
    }

    protected virtual void OnBeforeAttributeRemove(string attributeName)
    {
        if (attributeName.StartsWith(System.SysAttributeNamePrefix))
        {
            switch (attributeName)
            {
            }
        }
        else
        {
            switch (attributeName)
            {
            }
        }
    }

    protected virtual void OnAfterAttributeRemove(string attributeName)
    {
        if (attributeName.StartsWith(System.SysAttributeNamePrefix))
        {
            switch (attributeName)
            {
                case SystemAttributes.Indexed:
                Parent?.RemoveIndex(Key);
                break;
            }
        }
        else
        {
            switch (attributeName)
            {
            }
        }
    }

    #endregion

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