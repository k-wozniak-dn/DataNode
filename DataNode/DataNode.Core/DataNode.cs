
using System.Text.Json;

namespace DataNode.Core;

public interface IParentDataNode
{
    int GetIndex(string key);
    int SetIndex(string key);
    bool RemoveIndex(string key);
}

public class DataNode : IParentDataNode
{
    public DataNode() : base()
    {
    }

    #region Properties and Fields
    private readonly Dictionary<string, Item> Items = new([]);
    private readonly List<string> Index = new([]);

    #endregion

    #region Validate
    private void ValidateKeyCount(string key)
    {
        if (Count() >= System.ItemsCountLimit && !ContainsKey(key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {System.ItemsCountLimit}.");
        }
    }

    #endregion

    #region Items

    public int Count(bool includeSystemKeys = false)
    {
        return includeSystemKeys ? Items.Count : Items.Count(kvp => !kvp.Key.StartsWith(System.SysKeyPrefix));
    }

    public bool ContainsKey(string key)
    {
        key = Item.ValidateKey(key);
        return Items.ContainsKey(key);
    }

    public Dictionary<string, Item> Get()
    {
        var unindexed = Items
        .Where(kvp => !Index.Contains(kvp.Key))
        .OrderBy(kvp => kvp.Key);

        var indexed = Items
        .Where(kvp => Index.Contains(kvp.Key))
        .OrderBy(kvp => Index.IndexOf(kvp.Key));

        var result = unindexed.Concat(indexed).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return result;
    }
    
    public DataNode Set(Item item, bool existingOnly = false)
    {
        ValidateKeyCount(item.Key);

        if (existingOnly && !ContainsKey(item.Key))
        {
            throw new KeyNotFoundException($"Key '{item.Key}' not found. Use Add to create new key.");
        }

        if (item.Parent != null && item.Parent != this)
        {
            item = item.Copy(item.Key, this );
        }
        else item.Parent ??= this;

        Items[item.Key] = item;
        SetIndex(item.Key);
        return this;
    }

    public DataNode Add(Item item)
    {
        ValidateKeyCount(item.Key);

        if (item.Parent != null && item.Parent != this)
        {
            item = item.Copy(item.Key, this );
        }
        else item.Parent ??= this;

        Items.Add(item.Key, item);
        SetIndex(item.Key);
        return this;
    }

    public Item? Get(string key)
    {
        if (Items.TryGetValue(key, out Item? item))
        {
            return item;
        }
        else
        {
            return null;
        }
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
            item = new Item(key, this);
            Add(item);
            return item;
        }
    }

    public void Remove(string key)
    {
        Items.Remove(key);
        RemoveIndex(key);
    }

    public void Clear()
    {
        Items.Clear();
        Index.Clear();
    }

    #endregion

    #region Index

    public int SetIndex(string key)
    {

        if (key.StartsWith(System.SysKeyPrefix))
        {
            return -1; // System keys are not indexed
        }

        if (Index.Contains(key))
        {
            return Index.IndexOf(key);
        }
        else
        {
            Index.Add(key);
        }
        return Index.IndexOf(key);
    }

    public bool RemoveIndex(string key)
    {
        return Index.Remove(key);
    }

    private int MoveToIndex(string key, int newIndex)
    {
        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }
        if (newIndex < 0 || newIndex >= Index.Count)
        {
            throw new ArgumentOutOfRangeException($"New index '{newIndex}' is out of range.");
        }

        Index.Remove(key);
        Index.Insert(newIndex, key);
        return newIndex;
    }

    private void SetIndexAttributes()
    {
        Index.ForEach(key => Get(key)?.Set(SystemAttributes.Position, Index.IndexOf(key)));
    }

    private void ClearIndexAttributes()
    {
        Get().Where(kvp => kvp.Value.Contains(SystemAttributes.Position)).ToList()
        .ForEach(kvp => Get(kvp.Key)?.Remove(SystemAttributes.Position));
    } 
 
    public int GetIndex(string key)
    {
        return Index.IndexOf(key);
    }

    public int MoveToEnd(string key)
    {
        key = Item.ValidateKey(key);

        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        Index.Remove(key);
        Index.Add(key);
        return Index.Count - 1;
    }

    public int MoveToStart(string key)
    {
        key = Item.ValidateKey(key);

        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        Index.Remove(key);
        Index.Insert(0, key);
        return 0;
    }

    public int Move(string key, int offset)
    {
        key = Item.ValidateKey(key);

        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        var currentIndex = Index.IndexOf(key);
        var newIndex = Math.Max(0, Math.Min(Index.Count - 1, currentIndex + offset));

        return MoveToIndex(key, newIndex);
    }

    #endregion

    #region Export and Import

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

    #endregion
}