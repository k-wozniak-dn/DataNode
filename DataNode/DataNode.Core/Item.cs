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
    public int Count(bool includeSystemAttributes = false) { 
        return includeSystemAttributes ? Attributes.Count : Attributes.Count(kvp => !kvp.Value.IsSystemAttribute);
        }

    #endregion

    #region Validate
    public static string ValidateKey(string key)
    {
        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        return key.ToUpper();
    }
    public bool Contains(string attributeName)
    {
        attributeName = Attribute.ValidateName(attributeName);
        return Attributes.ContainsKey(attributeName);
    }
    private string ValidateAttributeCount(string attributeName)
    {
        if (Count() >= System.AttributesCountLimit && !Attributes.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {System.AttributesCountLimit}.");
        }
        return attributeName;
    }
    private string ValidateSetExisting(string attributeName, bool existingOnly)
    {
        if (existingOnly && !Contains(attributeName))
        {
            throw new InvalidOperationException($"Attribute '{attributeName}' does not exist. Use Add to add.");
        }
        return attributeName;
    }

    #endregion

    #region Constructors
    public Item(IEnumerable<Attribute> attributes, string? key, IParentDataNode? parent = null)
    {
        AddAll(attributes, true);
        if (key != null)
        {
            Set(SystemAttributes.Key, ValidateKey(key), false, true); 
        }
        Parent = parent;
    }

    #endregion

    #region Convertion
    public Dictionary<string, object> ToDictionary()
    {
        return GetAll().ToDictionary(attr => attr.Name, attr => DnValue.ToObjectValue(attr.Value));
    }
    public static Item FromDictionary(string key, Dictionary<string, object> dict)
    {
        var attributes = dict.Select(kvp => new Attribute(kvp.Key, DnValue.FromObjectValue(kvp.Value)));
        return new Item(attributes, key);
    }

    #endregion

    #region Copy
    public static Item Copy(Item from, string key, IParentDataNode? parent)
    {
        return new Item(from.GetAll(), key, parent);
    }
    public Item Copy(string? key = null, IParentDataNode? parent = null)
    {
        return Copy(this, key ?? Key, parent);
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
    public Attribute Set(Attribute attribute, bool existingOnly = false, bool skipHandlers = false)
    {
        ValidateSetExisting(ValidateAttributeCount(attribute.Name), existingOnly);
        attribute = TakeOwnership(attribute);
        if (!skipHandlers) { OnBeforeAttributeSet(attribute); }
        Attributes[attribute.Name] = attribute;
        if (!skipHandlers) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Set(string attributeName, object value, bool existingOnly = false, bool skipHandlers = false)
    {
        var attribute = new Attribute(attributeName, DnValue.FromObjectValue(value));
        return Set(attribute, existingOnly, skipHandlers);
    }
    public IEnumerable<Attribute> SetAll(IEnumerable<Attribute> attributes, bool existingOnly = false, bool skipHandlers = false )
    {
        foreach (var attribute in attributes)
        {
            Set(attribute, existingOnly, skipHandlers);
        }
        return attributes;
    }

    #endregion

    #region Add
    public Attribute Add(Attribute attribute, bool skipHandlers = false)
    {
        ValidateAttributeCount(attribute.Name);
        attribute = TakeOwnership(attribute);        
        if (!skipHandlers) { OnBeforeAttributeSet(attribute); }
        Attributes.Add(attribute.Name, attribute);
        if (!skipHandlers) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Add(string attributeName, object value, bool skipHandlers = false)
    {
        var attribute = new Attribute(attributeName, DnValue.FromObjectValue(value));
        return Add(attribute, skipHandlers);
    }
    public IEnumerable<Attribute> AddAll(IEnumerable<Attribute> attributes, bool skipHandlers = false )
    {
        foreach (var attribute in attributes)
        {
            Add(attribute, skipHandlers);
        }
        return attributes;
    }

    #endregion

    #region Remove
    public Attribute Remove(string attributeName, bool skipHandlers = false)
    {
        var attribute = Get(attributeName) ?? throw new InvalidOperationException($"Attribute '{attributeName}' does not exist.");
        if (!skipHandlers) { OnBeforeAttributeRemove(attribute); }
        Attributes.Remove(attribute.Name);
        if (!skipHandlers) { OnAfterAttributeRemove(attribute); }
        return attribute;
    }
    public Attribute Remove(Attribute attribute, bool skipHandlers = false)
    {
        return Remove(attribute.Name, skipHandlers);
    }
    public IEnumerable<Attribute> RemoveAll(IEnumerable<string> attributeNames, bool skipHandlers = false)
    {
        var removedAttributes = new List<Attribute>();
        foreach (var attributeName in attributeNames)
        {
            removedAttributes.Add(Remove(attributeName, skipHandlers));
        }
        return removedAttributes;
    }
    public IEnumerable<Attribute> RemoveAll(IEnumerable<Attribute> attributes, bool skipHandlers = false)
    {
        return RemoveAll(attributes.Select(attr => attr.Name), skipHandlers);
    }

    #endregion

    #region Handlers

    protected virtual void OnBeforeAttributeSet(Attribute attribute)
    {
        if (attribute.IsSystemAttribute)
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
        else
        {
            // switch (attribute.Name)
            // {
            // }
        }
    }

    protected virtual void OnAfterAttributeSet(Attribute attribute)
    {
        if (attribute.IsSystemAttribute)
        {
            switch (attribute.Name)
            {
                case SystemAttributes.Indexed:
                    Parent?.SetIndex(this);
                    break;
            }
        }
        else
        {
            // switch (attribute.Name)
            // {
            // }
        }
    }

    protected virtual void OnBeforeAttributeRemove(Attribute attribute)
    {
        if (attribute.IsSystemAttribute)
        {
            // switch (attribute.Name)
            // {
            // }
        }
        else
        {
            // switch (attribute.Name)
            // {
            // }
        }
    }

    protected virtual void OnAfterAttributeRemove(Attribute attribute)
    {
        if (attribute.IsSystemAttribute)
        {
            switch (attribute.Name)
            {
                case SystemAttributes.Indexed:
                Parent?.RemoveIndex(this);
                break;
            }
        }
        else
        {
            // switch (attribute.Name)
            // {
            // }
        }
    }

    #endregion

}