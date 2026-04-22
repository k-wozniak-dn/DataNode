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
    public Item Set(string attributeName, string value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        value = ValidateStringValue(value);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        var existing = Attributes.ContainsKey(attributeName);
        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute && existing)
        {
            OnBeforeSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnBeforeAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes[attributeName] = value;

        if (systemAttribute && existing)
        {
            OnAfterSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnAfterAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnAfterAttributeAdd(attributeName);
        }

        return this;
    }

    public Item Set(string attributeName, int value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        var existing = Attributes.ContainsKey(attributeName);
        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute && existing)
        {
            OnBeforeSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnBeforeAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes[attributeName] = value;

        if (systemAttribute && existing)
        {
            OnAfterSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnAfterAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnAfterAttributeAdd(attributeName);
        }
        return this;
    }

    public Item Set(string attributeName, decimal value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        var existing = Attributes.ContainsKey(attributeName);
        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute && existing)
        {
            OnBeforeSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnBeforeAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes[attributeName] = value;

        if (systemAttribute && existing)
        {
            OnAfterSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnAfterAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnAfterAttributeAdd(attributeName);
        }
        return this;
    }

    public Item Set(string attributeName, DnValue value, bool existingOnly = false)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);
        ValidateSetExisting(attributeName, existingOnly);

        var existing = Attributes.ContainsKey(attributeName);
        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute && existing)
        {
            OnBeforeSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnBeforeAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes[attributeName] = value;

        if (systemAttribute && existing)
        {
            OnAfterSystemAttributeSet(attributeName);
        }
        else if (systemAttribute && !existing)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else if (!systemAttribute && existing)
        {
            OnAfterAttributeSet(attributeName);
        }
        else if (!systemAttribute && !existing)
        {
            OnAfterAttributeAdd(attributeName);
        }
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

        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes.Add(attributeName, value);

        if (systemAttribute)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else
        {
            OnAfterAttributeAdd(attributeName);
        }
        return this;
    }

    public Item Add(string attributeName, int value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes.Add(attributeName, value);

        if (systemAttribute)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else
        {
            OnAfterAttributeAdd(attributeName);
        }
        return this;
    }

    public Item Add(string attributeName, decimal value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes.Add(attributeName, value);

        if (systemAttribute)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else
        {
            OnAfterAttributeAdd(attributeName);
        }
        return this;
    }

    public Item Add(string attributeName, DnValue value)
    {
        attributeName = ValidateAttributeName(attributeName);
        ValidateAttributeCount(attributeName);

        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute)
        {
            OnBeforeSystemAttributeAdd(attributeName);
        }
        else
        {
            OnBeforeAttributeAdd(attributeName);
        }

        Attributes.Add(attributeName, value);

        if (systemAttribute)
        {
            OnAfterSystemAttributeAdd(attributeName);
        }
        else
        {
            OnAfterAttributeAdd(attributeName);
        }
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

        var systemAttribute = SystemAttributes.All.Contains(attributeName);

        if (systemAttribute)
        {
            OnBeforeSystemAttributeRemove(attributeName);
        }
        else
        {
            OnBeforeAttributeRemove(attributeName);
        }

        Attributes.Remove(attributeName);

        if (systemAttribute)
        {
            OnAfterSystemAttributeRemove(attributeName);
        }
        else
        {
            OnAfterAttributeRemove(attributeName);
        }

        return this;
    }

    #region System Handlers

    protected virtual void OnBeforeSystemAttributeSet(string attributeName)
    {

    }

    protected virtual void OnAfterSystemAttributeSet(string attributeName)
    {
        switch (attributeName)
        {
            case SystemAttributes.Unindexed:
                Parent?.RemoveIndex(Key);
                break;
        }
    }

    protected virtual void OnBeforeSystemAttributeAdd(string attributeName)
    {

    }

    protected virtual void OnAfterSystemAttributeAdd(string attributeName)
    {
        switch (attributeName)
        {
            case SystemAttributes.Unindexed:
                Parent?.RemoveIndex(Key);
                break;
        }
    }

    protected virtual void OnBeforeSystemAttributeRemove(string attributeName)
    {

    }

    protected virtual void OnAfterSystemAttributeRemove(string attributeName)
    {
        switch (attributeName)
        {
            case SystemAttributes.Unindexed:
                Parent?.SetIndex(Key);
                break;
        }
    }

    #endregion

    #region Handlers

    protected virtual void OnBeforeAttributeSet(string attributeName)
    {

    }

    protected virtual void OnAfterAttributeSet(string attributeName)
    {

    }

    protected virtual void OnBeforeAttributeAdd(string attributeName)
    {

    }

    protected virtual void OnAfterAttributeAdd(string attributeName)
    {

    }

    protected virtual void OnBeforeAttributeRemove(string attributeName)
    {

    }

    protected virtual void OnAfterAttributeRemove(string attributeName)
    {

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