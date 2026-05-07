
using System.Dynamic;
using System.Text.Json;

namespace DataNode.Core;

public interface IParentDataNode
{
    int GetIndex(string key);
    Item SetIndex(Item item);
    Item RemoveIndex(Item item);
}

public class DataNode : IParentDataNode
{
    #region Properties
    private Dictionary<string, Item> Items { get; } = new([]);
    private List<string> Index { get; } = new([]);

    #endregion

    #region Constructors
    public DataNode()
    {
    }
    public DataNode(IEnumerable<Item> items) : base()
    {
        foreach (var item in OrderItems(items)) { AddCore(item); }
    }

    #endregion  

    #region Query Properties

    public bool ContainsKey(string key)
    {
        return Items.ContainsKey(Item.ValidateKey(key));
    }
    public int CountSys
    {
        get
        {
            return Items.Count(kvp => kvp.Value.IsSystemItem);            
        }
    }
    public int CountNonSys
    {
        get
        {
            return Items.Count(kvp => !kvp.Value.IsSystemItem);            
        }
    }
    public int CountAll
    {
        get
        {
            return Items.Count;            
        }
    }

    #endregion 

    #region Validate
    private Item ValidateKeyCount(Item item)
    {
        if (!item.IsSystemItem && CountNonSys >= System.ItemsCountLimit && !ContainsKey(item.Key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {System.ItemsCountLimit}.");
        }
        return item;
    }
    private Item ValidateExisting(Item item, bool existingOnly)
    {
        if (existingOnly && !ContainsKey(item.Key))
        {
            throw new KeyNotFoundException($"Key '{item.Key}' not found. Use Add to create new key.");
        }
        return item;
    }
    private Item ValidateIndexed(Item item)
    {
        if (!ContainsKey(item.Key) || !item.Contains(SystemAttributes.Indexed) || item.Parent != this)
        {
            throw new InvalidOperationException($"Item '{item.Key}' can't be indexed.");
        }
        return item;
    }
    public static Item ValidateNonSys(Item item)
    {
        if (SystemKeys.All.Contains(item.Key))
        {
            throw new ArgumentException($"System item {item.Key} not allowed for this method.");
        }
        return item;
    }
    
    #endregion

    #region Copy
    public static DataNode Copy(DataNode from)
    {
        return new DataNode(from.GetAll());
    }
    public DataNode Copy()
    {
        return Copy(this);
    }
    
    #endregion

    #region Get
    public static IEnumerable<Item> OrderItems(IEnumerable<Item> items)
    {
        var unindexed = items.Where(item => !item.Contains(SystemAttributes.Indexed))
            .OrderBy(item => item.Key);
        var indexed = items.Where(item => item.Contains(SystemAttributes.Indexed))
            .OrderBy(item => item.Get(SystemAttributes.Indexed)?.GetInteger());
        var result = unindexed.Concat(indexed).ToArray();
        return result;
    }  
    public IEnumerable<Item> GetAll()
    {
        return OrderItems(Items.Values);
    }
    public Item? Get(string key)
    {
        if (Items.TryGetValue(Item.ValidateKey(key), out Item? item))
        {
            return item;
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Set
    public Item TakeOwnership(Item item)
    {
        return (item.Parent != this) ? item.Copy(parent: this) : item;
    }
    private Item SetCore(Item item, bool existingOnly = false, bool skipHandlers = false)
    {
        item = ValidateExisting(ValidateKeyCount(item), existingOnly);
        item = TakeOwnership(item);
        if (!skipHandlers && !item.IsSystemItem) { OnBeforeItemSet(item); }
        if (!skipHandlers && item.IsSystemItem) { OnBeforeSysItemSet(item); }
        Items[item.Key] = item;
        if (!skipHandlers && item.IsSystemItem) { OnAfterSysItemSet(item); }
        if (!skipHandlers && !item.IsSystemItem) { OnAfterItemSet(item); }
        return item;
    }
    public Item Set(Item item, bool existingOnly = false)
    {
        item = ValidateNonSys(item);
        return SetCore(item, existingOnly) ;
    }
    public IEnumerable<Item> SetAll(IEnumerable<Item> items, bool existingOnly = false)
    {
        var result = new List<Item>();
        foreach (var item in items)
        {
            result.Add(Set(item, existingOnly));
        }
        return result;
    }

    #endregion

    #region Add

    public Item AddCore(Item item, bool skipHandlers = false)
    {
        item = ValidateKeyCount(item);
        item = TakeOwnership(item);
        if (!skipHandlers && !item.IsSystemItem) { OnBeforeItemSet(item); }
        if (!skipHandlers && item.IsSystemItem) { OnBeforeSysItemSet(item); }
        Items.Add(item.Key, item);
        if (!skipHandlers && item.IsSystemItem) { OnAfterSysItemSet(item); }
        if (!skipHandlers && !item.IsSystemItem) { OnAfterItemSet(item); }
        return item;
    }
    public Item Add(Item item)
    {
        item = ValidateNonSys(item);
        return AddCore(item) ;
    }
    public IEnumerable<Item> AddAll(IEnumerable<Item> items)
    {
        var result = new List<Item>();
        foreach (var item in items)
        {
            result.Add(Add(item));
        }
        return result;
    }
    public Item GetOrCreate(string key)
    {
        var item = Get(key);
        if (item != null)
        {
            return item;
        }
        else
        {
            item = new Item([], key, this);
            return Add(item);
        }
    }

    #endregion

    #region Remove

    public Item RemoveCore(string key, bool skipHandlers = false)
    {
        var item = Get(key) ?? throw new InvalidOperationException($"Item '{key}' does not exist.");
        if (!skipHandlers && !item.IsSystemItem) { OnBeforeItemRemove(item); }
        if (!skipHandlers && item.IsSystemItem) { OnBeforeSysItemRemove(item); }        
        Items.Remove(item.Key);
        if (!skipHandlers && item.IsSystemItem) { OnAfterSysItemRemove(item); }        
        if (!skipHandlers && !item.IsSystemItem) { OnAfterItemRemove(item); }
        return item;
    }
    public Item Remove(string key)
    {
        var item = Get(key) ?? throw new InvalidOperationException($"Item '{key}' does not exist.");
        item = ValidateNonSys(item);
        return RemoveCore(item.Key) ;
    }
    public Item Remove(Item item)
    {
        return Remove(item.Key);
    }
    public IEnumerable<Item> RemoveAll(IEnumerable<string> keys, bool skipHandlers = false)
    {
        var removedItems = new List<Item>();
        foreach (var key in keys)
        {
            removedItems.Add(Remove(key));
        }
        return removedItems;
    }
    public IEnumerable<Item> RemoveAll(IEnumerable<Item> items, bool skipHandlers = false)
    {
        return RemoveAll(items.Select(item => item.Key), skipHandlers);
    }
    public IEnumerable<Item> Clear( bool skipHandlers = false)
    {
        return RemoveAll(Items.Keys.ToArray(), skipHandlers);
    }
    #endregion

    #region Sys Handlers

    private void OnBeforeSysItemSet(Item item)
    {
        if (item.Contains(SystemAttributes.Indexed))
        {
            throw new InvalidOperationException($"Attribute '{SystemAttributes.Indexed}' cannot be set on system keys.");
        }
    }

    private void OnAfterSysItemSet(Item item)
    {
    }

    private void OnBeforeSysItemRemove(Item item)
    {
    }

    private void OnAfterSysItemRemove(Item item)
    {
    }

    #endregion

    #region Handlers

    protected virtual void OnBeforeItemSet(Item item)
    {
    }

    protected virtual void OnAfterItemSet(Item item)
    {
        if (item.Contains(SystemAttributes.Indexed))
        {
            SetIndex(item);
        }
    }

    protected virtual void OnBeforeItemRemove(Item item)
    {
    }

    protected virtual void OnAfterItemRemove(Item item)
    {
        if (item.Contains(SystemAttributes.Indexed))
        {
            RemoveIndex(item);
        }
    }

    #endregion

    #region Index
    public int GetIndex(string key)
    {
        return Index.IndexOf(key);  
    }
    private void ResetIndexedAttributes()
    {
        Index.ForEach(key =>
        {
            var item = Get(key);
            item?.RefreshIndexAttribute();
        });
    }
    public Item SetIndex(Item item)
    {
        ValidateIndexed(item);
        if (!Index.Contains(item.Key))
        {
            Index.Add(item.Key);
        }
        ResetIndexedAttributes();
        return item;
    }
    public Item RemoveIndex(Item item)
    {
        Index.Remove(item.Key);
        ResetIndexedAttributes();
        return item;
    }
    private Item MoveToIndex(Item item, int newIndex)
    {
        ValidateIndexed(item);
        Index.Remove(item.Key);
        Index.Insert(newIndex, item.Key);
        ResetIndexedAttributes();
        return item;
    }
    public Item MoveToEnd(Item item)
    {
        return MoveToIndex(item, Index.Count - 1);
    }
    public Item MoveToStart(Item item)
    {
        return MoveToIndex(item, 0);
    }
    public Item Move(Item item, int offset)
    {
        var currentIndex = Index.IndexOf(item.Key);
        var newIndex = Math.Max(0, Math.Min(Index.Count - 1, currentIndex + offset));
        return MoveToIndex(item, newIndex);
    }

    #endregion

    #region Convertion
    public Dictionary<string, Dictionary<string, object>> ToDictionary()
    {
        return GetAll().ToDictionary(item => item.Key, item => item.ToDictionary());
    }
    public static DataNode FromDictionary(Dictionary<string, Dictionary<string, object>> dict)
    {
        var items = dict.Select(kvp => Item.FromDictionary(kvp.Value));
        return new DataNode(items);
    }

    #endregion


    // public IEnumerable<KeyValuePair<string, Item>> ExportKeys()
    // {
    //     SetIndexAttributes();
    //     var result = Get();
    //     ClearIndexAttributes();
    //     return result;
    // }

    // public void ImportKeys(IEnumerable<KeyValuePair<string, Item>> data)
    // {
    //     Clear();

    //     var sys = data
    //     .Where(kvp => kvp.Key.StartsWith(System.SysKeyPrefix))
    //     .OrderBy(kvp => kvp.Key);

    //     var nonIndexed = data
    //     .Where(kvp => kvp.Key.StartsWith(System.NoIndexKeyPrefix))
    //     .OrderBy(kvp => kvp.Key);

    //     var indexed = data
    //     .Where(kvp => kvp.Value.Contains(SystemAttributes.Position)) 
    //     .OrderBy(kvp => kvp.Value.GetInteger(SystemAttributes.Position));

    //     var other = data
    //     .Where(kvp => 
    //         !kvp.Key.StartsWith(System.SysKeyPrefix) && 
    //         !kvp.Key.StartsWith(System.NoIndexKeyPrefix) && 
    //         !kvp.Value.Contains(SystemAttributes.Position))
    //     .OrderBy(kvp => kvp.Key);

    //     var import = sys.Concat(nonIndexed).Concat(indexed).Concat(other).ToArray();

    //     foreach (var kvp in import)
    //     {
    //         Add(kvp.Key, kvp.Value);
    //     }

    //     ClearIndexAttributes();
    // }

    // public string ExportJson()
    // {
    //     // Convert to a dictionary for cleaner JSON structure
    //     var jsonObject = new Dictionary<string, Dictionary<string, object>>();

    //     foreach (var kvp in ExportKeys())
    //     {
    //         var attributesDict = new Dictionary<string, object>();
    //         foreach (var attr in kvp.Value.GetAll())
    //         {
    //             // Extract the value from DnValue
    //             object? value = null;
    //             if (attr.Value is StringValue sv)
    //             {
    //                 value = sv.Value;
    //             }
    //             else if (attr.Value is IntegerValue iv)
    //             {
    //                 value = iv.Value;
    //             }
    //             else if (attr.Value is DecimalValue dv)
    //             {
    //                 value = dv.Value;
    //             }
    //             if (value != null)
    //             {
    //                 attributesDict[attr.Key] = value;
    //             }
    //         }
    //         jsonObject[kvp.Key] = attributesDict;
    //     }
    //     return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
    // }

    // public DataNode ImportJson(string json)
    // {
    //     var jsonObject = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(json) 
    //         ?? throw new ArgumentException("Invalid JSON format.");

    //     var data = new Dictionary<string, Item>();

    //     foreach (var kvp in jsonObject)
    //     {
    //         var key = kvp.Key;
    //         var attributesData = kvp.Value;
    //         var attributes =  new Item(key, this);

    //         foreach (var attrKvp in attributesData)
    //         {
    //             var attributeName = attrKvp.Key;
    //             var jsonValue = attrKvp.Value;

    //             try
    //             {
    //                 // Determine the type and set the appropriate value
    //                 if (jsonValue.ValueKind == JsonValueKind.Number)
    //                 {
    //                     // Try to parse as int first, then decimal
    //                     if (jsonValue.TryGetInt32(out int intValue))
    //                     {
    //                         attributes.Set(attributeName, intValue);
    //                     }
    //                     else if (jsonValue.TryGetDecimal(out decimal decimalValue))
    //                     {
    //                         attributes.Set(attributeName, decimalValue);
    //                     }
    //                 }
    //                 else if (jsonValue.ValueKind == JsonValueKind.String)
    //                 {
    //                     var stringValue = jsonValue.GetString();
    //                     if (stringValue != null)
    //                     {
    //                         attributes.Set(attributeName, stringValue);
    //                     }
    //                 }
    //             }
    //             catch (Exception ex)
    //             {
    //                 throw new InvalidOperationException($"Error importing attribute '{attributeName}' for key '{key}': {ex.Message}", ex);
    //             }
    //         }
    //         data[key] = attributes;
    //     }

    //     ImportKeys(data);
    //     return this;
    // }

}