using System.Runtime.CompilerServices;

namespace DataNode.Core;

public interface IItem
{
    string Key { get; }
}
public class Item : IItem
{
    #region Properties
    private Dictionary<string, Attribute> Attributes { get; } = new([]);
    public string Key { 
        get 
        {
            return Get(SystemAttributes.Key)?.GetString() ?? throw new KeyNotFoundException("Key not found.");
        } 
    }
    public IParentDataNode? Parent { get; }

    #endregion

    #region Constructors
    public Item(IEnumerable<Attribute> attributes, string? key = null, IParentDataNode? parent = null)
    {
        if (key != null) key = ValidateKey(key);

        foreach (var attr in attributes) { AddCore(attr, true); }
        if (key != null) { SetCore(new Attribute(SystemAttributes.Key, key), false, true); }
        Parent = parent;
    }

    #endregion   

    #region Validate
    public static string ValidateKey(string key)
    {
        key = key.Trim().ToUpper();
        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        return key;
    }
    private Attribute ValidateAttributeCount(Attribute attribute)
    {
        if (CountNonSys >= System.AttributesCountLimit && !Contains(attribute.Name))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
        return attribute;
    }
    private Attribute ValidateSetExisting(Attribute attribute, bool existingOnly)
    {
        if (existingOnly && !Contains(attribute.Name))
        {
            throw new InvalidOperationException($"Attribute '{attribute.Name}' does not exist. Use Add to add.");
        }
        return attribute;
    }
    public static Attribute ValidateNonSys(Attribute attribute)
    {
        if (SystemAttributes.All.Contains(attribute.Name))
        {
            throw new ArgumentException($"System attribute {attribute.Name} not allowed for this method.");
        }
        return attribute;
    }

    #endregion

    #region QueryProperties
    public bool Contains(string attributeName)
    {
        attributeName = Attribute.ValidateName(attributeName);
        return Attributes.ContainsKey(attributeName);
    }
    public int? IndexPosition
    {
        get
        {
            return Parent?.GetIndex(Key);
        }
    }
    public bool IsSystemItem {
        get 
        {
            return Key.StartsWith(System.SysKeyPrefix);
        }
    }
    public int CountSys
    {
        get
        {
            return Attributes.Count(kvp => kvp.Value.IsSystemAttribute);
        }
    }
    public int CountNonSys
    {
        get
        {
            return Attributes.Count(kvp => !kvp.Value.IsSystemAttribute);
        }
    }
    public int CountAll
    {
        get
        {
            return Attributes.Count;
        }
    }

    #endregion

    #region Copy
    public static Item Copy(Item from, string? key = null, IParentDataNode? parent = null)
    {
        return new Item(from.GetAll(), key, parent);
    }
    public Item Copy(string? key = null, IParentDataNode? parent = null)
    {
        return Copy(this, key, parent);
    }

    #endregion

    #region Get
    public IEnumerable<Attribute> GetAll()
    {
        return [.. Attributes.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value)];
    }
    public Attribute? Get(string attributeName)
    {
        attributeName = Attribute.ValidateName(attributeName);
        if (Attributes.TryGetValue(attributeName, out Attribute? attribute))
        {
            return attribute;
        }
        else
        {
            return null;
        }
    }
    public Attribute GetOrDefault(string attributeName, object defaultValue)
    {
        attributeName = Attribute.ValidateName(attributeName);
        if (Attributes.TryGetValue(attributeName, out Attribute? attribute))
        {
            return attribute;
        }
        else
        {
            return new Attribute(attributeName, DnValue.FromObjectValue(defaultValue));
        }
    }

    #endregion

    #region Set
    public Attribute TakeOwnership(Attribute attribute)
    {
        return (attribute.Parent != this) ? attribute.Copy(parent: this) : attribute;
    }
    private Attribute SetCore(Attribute attribute, bool existingOnly = false, bool skipHandlers = false)
    {
        attribute = ValidateSetExisting(ValidateAttributeCount(attribute), existingOnly);
        attribute = TakeOwnership(attribute);
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnBeforeAttributeSet(attribute); }
        if (!skipHandlers && attribute.IsSystemAttribute) { OnBeforeSysAttributeSet(attribute); }
        Attributes[attribute.Name] = attribute;
        if (!skipHandlers && attribute.IsSystemAttribute) { OnAfterSysAttributeSet(attribute); }
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Set(Attribute attribute, bool existingOnly = false)
    {
        attribute = ValidateNonSys(attribute);
        return SetCore(attribute, existingOnly);
    }
    public Attribute Set(string attributeName, object value, bool existingOnly = false)
    {
        var attribute = new Attribute(attributeName, DnValue.FromObjectValue(value));
        return Set(attribute, existingOnly);
    }
    public IEnumerable<Attribute> SetAll(IEnumerable<Attribute> attributes, bool existingOnly = false )
    {
        foreach (var attribute in attributes)
        {
            Set(attribute, existingOnly);
        }
        return attributes;
    }
    public Attribute? RefreshIndexAttribute()
    {
        var idxAttribute = Get(SystemAttributes.Indexed);
        if (idxAttribute != null && idxAttribute.GetInteger() != IndexPosition)
        {
            SetCore(new Attribute(SystemAttributes.Indexed, IndexPosition ?? -1), true, true);
        }
        return idxAttribute;
    }
    #endregion

    #region Add
    private Attribute AddCore(Attribute attribute, bool skipHandlers = false)
    {
        attribute = ValidateAttributeCount(attribute);
        attribute = TakeOwnership(attribute);
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnBeforeAttributeSet(attribute); }
        if (!skipHandlers && attribute.IsSystemAttribute) { OnBeforeSysAttributeSet(attribute); }
        Attributes[attribute.Name] = attribute;
        if (!skipHandlers && attribute.IsSystemAttribute) { OnAfterSysAttributeSet(attribute); }
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Add(Attribute attribute)
    {
        attribute = ValidateNonSys(attribute);
        return AddCore(attribute);
    }
    public Attribute Add(string attributeName, object value)
    {
        var attribute = new Attribute(attributeName, DnValue.FromObjectValue(value));
        return Add(attribute);
    }
    public IEnumerable<Attribute> AddAll(IEnumerable<Attribute> attributes )
    {
        foreach (var attribute in attributes)
        {
            Add(attribute);
        }
        return attributes;
    }

    #endregion

    #region Remove
    private Attribute RemoveCore(string attributeName, bool skipHandlers = false)
    {
        var attribute = Get(attributeName) ?? throw new InvalidOperationException($"Attribute '{attributeName}' does not exist.");
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnBeforeAttributeRemove(attribute); }
        if (!skipHandlers && attribute.IsSystemAttribute) { OnBeforeSysAttributeRemove(attribute); }
        Attributes.Remove(attribute.Name);
        if (!skipHandlers && attribute.IsSystemAttribute) { OnAfterSysAttributeRemove(attribute); }
        if (!skipHandlers && !attribute.IsSystemAttribute) { OnAfterAttributeRemove(attribute); }
        return attribute;
    }
    public Attribute Remove(string attributeName)
    {
        var attribute = Get(attributeName) ?? throw new InvalidOperationException($"Attribute '{attributeName}' does not exist.");
        attribute = ValidateNonSys(attribute);
        return RemoveCore(attribute.Name);
    }
    public Attribute Remove(Attribute attribute)
    {
        return Remove(attribute.Name);
    }
    public IEnumerable<Attribute> RemoveAll(IEnumerable<string> attributeNames)
    {
        var removedAttributes = new List<Attribute>();
        foreach (var attributeName in attributeNames)
        {
            removedAttributes.Add(Remove(attributeName));
        }
        return removedAttributes;
    }
    public IEnumerable<Attribute> RemoveAll(IEnumerable<Attribute> attributes)
    {
        return RemoveAll(attributes.Select(attr => attr.Name));
    }

    #endregion

    #region Sys Handlers

    private void OnBeforeSysAttributeSet(Attribute attribute)
    {
        switch (attribute.Name)
        {
            case SystemAttributes.Indexed:
                if (IsSystemItem)
                {
                    throw new InvalidOperationException($"Attribute '{attribute.Name}' cannot be set in system item.");
                }
                break;
        }
    }

    private void OnAfterSysAttributeSet(Attribute attribute)
    {
        switch (attribute.Name)
        {
            case SystemAttributes.Indexed:
                Parent?.SetIndex(this);
                break;
        }
    }

    private void OnBeforeSysAttributeRemove(Attribute attribute)
    {
    }

    private void OnAfterSysAttributeRemove(Attribute attribute)
    {
        switch (attribute.Name)
        {
            case SystemAttributes.Indexed:
                Parent?.RemoveIndex(this);
                break;
        }
    }

    #endregion

    #region Handlers

    protected virtual void OnBeforeAttributeSet(Attribute attribute)
    {
    }

    protected virtual void OnAfterAttributeSet(Attribute attribute)
    {
    }

    protected virtual void OnBeforeAttributeRemove(Attribute attribute)
    {
    }

    protected virtual void OnAfterAttributeRemove(Attribute attribute)
    {
    }

    #endregion

    #region Convertion
    public Dictionary<string, object> ToDictionary()
    {
        return GetAll().ToDictionary(attr => attr.Name, attr => DnValue.ToObjectValue(attr.Value));
    }
    public static Item FromDictionary(Dictionary<string, object> dict)
    {
        var attributes = dict.Select(kvp => new Attribute(kvp.Key, DnValue.FromObjectValue(kvp.Value)));
        return new Item(attributes);
    }

    #endregion
}