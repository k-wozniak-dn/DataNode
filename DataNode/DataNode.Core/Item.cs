namespace DataNode.Core;

public interface IItem
{
}
public class Item(IEnumerable<Attribute> attributes, string key, IParentDataNode? parent = null) : IItem
{

    #region Properties
    public Dictionary<string, Attribute> Attributes { get; } = attributes.ToDictionary(attr => attr.Name, attr => attr);
    public string Key { get; } = ValidateKey(key);
    public IParentDataNode? Parent { get; set; } = parent;
    public int? IndexPosition
    {
        get
        {
            return Parent?.GetIndex(Key);
        }
    }

    #endregion

    public bool Contains(string attributeName)
    {
        attributeName = Attribute.ValidateName(attributeName);
        return Attributes.ContainsKey(attributeName);
    }
    public static Item Copy(Item from, string key, IParentDataNode? parent)
    {
        return new Item(from.GetAll(), key, parent);
    }
    public Item Copy(string? key = null, IParentDataNode? parent = null)
    {
        return Copy(this, key ?? Key, parent);
    }
    public int Count(bool includeSystemAttributes = false) { 
        return includeSystemAttributes ? Attributes.Count : Attributes.Count(kvp => !kvp.Value.IsSystemAttribute);
        }
    public bool IsSystemItem {
        get 
        {
            return Key.StartsWith(System.SysKeyPrefix);
        }
    }
    public Item TakeOwnership(IParentDataNode parent)
    {
        return (Parent != parent) ? Copy(parent: parent) : this;
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

    public Attribute GetOrDefault(string attributeName, DnValue defaultValue)
    {   
        attributeName = Attribute.ValidateName(attributeName);
        if (Attributes.TryGetValue(attributeName, out Attribute? attribute))
        {
            return attribute;
        }
        else
        {
            return new Attribute(attributeName, defaultValue);
        }
    }

    #endregion

    #region Set

    public Attribute Set(Attribute attribute, bool existingOnly = false, bool skipHandlers = false)
    {
        ValidateSetExisting(ValidateAttributeCount(attribute.Name), existingOnly);
        attribute.TakeOwnership(this);        
        if (!skipHandlers) { OnBeforeAttributeSet(attribute); }
        Attributes[attribute.Name] = attribute;
        if (!skipHandlers) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Set(string attributeName, DnValue value, bool existingOnly = false, bool skipHandlers = false)
    {
        var attribute = new Attribute(attributeName, value);
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
        attribute.TakeOwnership(this);        
        if (!skipHandlers) { OnBeforeAttributeSet(attribute); }
        Attributes.Add(attribute.Name, attribute);
        if (!skipHandlers) { OnAfterAttributeSet(attribute); }
        return attribute;
    }
    public Attribute Add(string attributeName, DnValue value, bool skipHandlers = false)
    {
        var attribute = new Attribute(attributeName, value);
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