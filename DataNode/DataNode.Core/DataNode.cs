
using System.Text.Json;

namespace DataNode.Core;

public interface IParentDataNode
{
    int GetIndex(string key);
}

public class DataNode : IParentDataNode
{
    public DataNode() : base()
    {
    }

    #region Properties and Fields
    private readonly Dictionary<string, Item> Keys = new([]);
    private readonly List<string> Index = new([]);
    public int CountNonSys { get { return Keys.Count(kvp => !kvp.Key.StartsWith(System.SysKeyPrefix)) ;} }  

    #endregion

    #region Keys

    private static string ValidateKey(string key)
    {
        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        return key.ToUpper();
    }
   
    public IEnumerable<KeyValuePair<string, Item>> GetAll()
    {
        var sys = Keys
        .Where(kvp => kvp.Key.StartsWith(System.SysKeyPrefix))
        .OrderBy(kvp => kvp.Key);

        var nonIndexed = Keys
        .Where(kvp => kvp.Key.StartsWith(System.NoIndexKeyPrefix))
        .OrderBy(kvp => kvp.Key);

        var indexed = Keys
        .Where(kvp => Index.Contains(kvp.Key))
        .OrderBy(kvp => Index.IndexOf(kvp.Key));

        var result = sys.Concat(nonIndexed).Concat(indexed).ToArray();
        return result;
    }
    
    public bool ContainsKey(string key)
    {
        key = ValidateKey(key);
        return Keys.ContainsKey(key);
    }

    public DataNode Set(string key, Item item, bool existingOnly = false)
    {
        key = ValidateKey(key);

        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        if (CountNonSys >= System.ItemsCountLimit && !ContainsKey(key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {System.ItemsCountLimit}.");
        }
        if (existingOnly && !ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found. Use Add to create new key.");
        }
        if (item.Parent != this)
        {
            item = item.Copy(key, this );
        }

        Keys[key] = item;
        SetIndex(key);
        return this;
    }

    public DataNode Add(string key, Item attributes)
    {
        key = ValidateKey(key);

        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        if (CountNonSys >= System.ItemsCountLimit && !ContainsKey(key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {System.ItemsCountLimit}.");
        }
        if (attributes.Parent != this)
        {
            attributes = attributes.Copy( key, this);
        }
        Keys.Add(key, attributes);
        SetIndex(key);
        return this;
    }

    public Item? Get(string key)
    {
        key = ValidateKey(key);

        if (Keys.TryGetValue(key, out Item? attributes))
        {
            return attributes;
        }
        else
        {
            return null;
        }
    }

    public Item GetOrCreate(string key)
    {
        key = ValidateKey(key);

        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes;
        }
        else
        {
            attributes = new Item(key, this);
            Add(key, attributes);
            return attributes;
        }
    }

    public void Remove(string key)
    {
        key = ValidateKey(key);
        Keys.Remove(key);
        RemoveIndex(key);
    }

    public void Clear()
    {
        Keys.Clear();
        Index.Clear();
    }

    #endregion

    #region Attribute

    public void Set(string key, string attributeName, string value, bool existingOnly = false)
    {
        var attributes = existingOnly ? Get(key) : GetOrCreate(key);
        if (attributes != null)
        {
            attributes.Set(attributeName, value, existingOnly);
        }
        else
        {
            throw new KeyNotFoundException($"Key '{key}' not found. Use Add to create new key.");
        }
    }

    public void Set(string key, string attributeName, int value, bool existingOnly = false)
    {
        var attributes = existingOnly ? Get(key) : GetOrCreate(key);
        if (attributes != null)
        {
            attributes.Set(attributeName, value, existingOnly);
        }
        else
        {
            throw new KeyNotFoundException($"Key '{key}' not found. Use Add to create new key.");
        }
    }

    public void Set(string key, string attributeName, decimal value, bool existingOnly = false)
    {
        var attributes = existingOnly ? Get(key) : GetOrCreate(key);
        if (attributes != null)
        {
            attributes.Set(attributeName, value, existingOnly);
        }
        else
        {
            throw new KeyNotFoundException($"Key '{key}' not found. Use Add to create new key.");
        }
    }

    public void Add(string key, string attributeName, string value)
    {
        var attributes = GetOrCreate(key);
        attributes.Add(attributeName, value);
    }

    public void Add(string key, string attributeName, int value)
    {
        var attributes = GetOrCreate(key);
        attributes.Add(attributeName, value);
    }

    public void Add(string key, string attributeName, decimal value)
    {
        var attributes = GetOrCreate(key);
        attributes.Add(attributeName, value);
    }

    public DnValue? Get(string key, string attributeName)
    {
        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes.Get(attributeName);
        }
        else
        {
            return null;
        }
    }

    public string? GetString(string key, string attributeName)
    {
        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes.GetString(attributeName);
        }
        else
        {
            return null;
        }
    }

    public int? GetInteger(string key, string attributeName)
    {
        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes.GetInteger(attributeName);
        }
        else
        {
            return null;
        }
    }

    public decimal? GetDecimal(string key, string attributeName)
    {
        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes.GetDecimal(attributeName);
        }
        else
        {
            return null;
        }
    }

    public void Remove(string key, string attributeName)
    {
        var attributes = Get(key);
        attributes?.Remove(attributeName);
    }

    public bool Contains(string key, string attributeName)
    {
        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes.Contains(attributeName);
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Index

    public int GetIndex(string key)
    {
        return Index.IndexOf(key);
    }
    public int SetIndex(string key)
    {
        key = ValidateKey(key);

        if (key.StartsWith(System.SysKeyPrefix))
        {
            return -1; // System keys are not indexed
        }

        if (key.StartsWith(System.NoIndexKeyPrefix))
        {
            return -1; // Key is not indexed
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
        key = ValidateKey(key);
        return Index.Remove(key);
    }

    public int MoveToIndex(string key, int newIndex)
    {
        key = ValidateKey(key);

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

    public int MoveToEnd(string key)
    {
        key = ValidateKey(key);

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
        key = ValidateKey(key);

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
        key = ValidateKey(key);

        if (!ContainsKey(key))
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        var currentIndex = Index.IndexOf(key);
        var newIndex = Math.Max(0, Math.Min(Index.Count - 1, currentIndex + offset));

        return MoveToIndex(key, newIndex);
    }

    public void SetIndexAttributes()
    {
        Index.ForEach(key => Set(key, SystemAttributes.Position, Index.IndexOf(key)));
    }

    public void ClearIndexAttributes()
    {
        GetAll().Where(kvp => kvp.Value.Contains(SystemAttributes.Position)).ToList()
        .ForEach(kvp => Remove(kvp.Key, SystemAttributes.Position));
    }
    #endregion

    #region Export and Import

    public IEnumerable<KeyValuePair<string, Item>> ExportKeys()
    {
        SetIndexAttributes();
        var result = GetAll();
        ClearIndexAttributes();
        return result;
    }

    public void ImportKeys(IEnumerable<KeyValuePair<string, Item>> data)
    {
        Clear();

        var sys = data
        .Where(kvp => kvp.Key.StartsWith(System.SysKeyPrefix))
        .OrderBy(kvp => kvp.Key);

        var nonIndexed = data
        .Where(kvp => kvp.Key.StartsWith(System.NoIndexKeyPrefix))
        .OrderBy(kvp => kvp.Key);

        var indexed = data
        .Where(kvp => kvp.Value.Contains(SystemAttributes.Position)) 
        .OrderBy(kvp => kvp.Value.GetInteger(SystemAttributes.Position));

        var other = data
        .Where(kvp => 
            !kvp.Key.StartsWith(System.SysKeyPrefix) && 
            !kvp.Key.StartsWith(System.NoIndexKeyPrefix) && 
            !kvp.Value.Contains(SystemAttributes.Position))
        .OrderBy(kvp => kvp.Key);

        var import = sys.Concat(nonIndexed).Concat(indexed).Concat(other).ToArray();

        foreach (var kvp in import)
        {
            Add(kvp.Key, kvp.Value);
        }

        ClearIndexAttributes();
    }

    public string ExportJson()
    {
        // Convert to a dictionary for cleaner JSON structure
        var jsonObject = new Dictionary<string, Dictionary<string, object>>();

        foreach (var kvp in ExportKeys())
        {
            var attributesDict = new Dictionary<string, object>();
            foreach (var attr in kvp.Value.GetAll())
            {
                // Extract the value from DnValue
                object? value = null;
                if (attr.Value is StringValue sv)
                {
                    value = sv.Value;
                }
                else if (attr.Value is IntegerValue iv)
                {
                    value = iv.Value;
                }
                else if (attr.Value is DecimalValue dv)
                {
                    value = dv.Value;
                }
                if (value != null)
                {
                    attributesDict[attr.Key] = value;
                }
            }
            jsonObject[kvp.Key] = attributesDict;
        }
        return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
    }

    public DataNode ImportJson(string json)
    {
        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(json) 
            ?? throw new ArgumentException("Invalid JSON format.");

        var data = new Dictionary<string, Item>();

        foreach (var kvp in jsonObject)
        {
            var key = kvp.Key;
            var attributesData = kvp.Value;
            var attributes =  new Item(key, this);

            foreach (var attrKvp in attributesData)
            {
                var attributeName = attrKvp.Key;
                var jsonValue = attrKvp.Value;

                try
                {
                    // Determine the type and set the appropriate value
                    if (jsonValue.ValueKind == JsonValueKind.Number)
                    {
                        // Try to parse as int first, then decimal
                        if (jsonValue.TryGetInt32(out int intValue))
                        {
                            attributes.Set(attributeName, intValue);
                        }
                        else if (jsonValue.TryGetDecimal(out decimal decimalValue))
                        {
                            attributes.Set(attributeName, decimalValue);
                        }
                    }
                    else if (jsonValue.ValueKind == JsonValueKind.String)
                    {
                        var stringValue = jsonValue.GetString();
                        if (stringValue != null)
                        {
                            attributes.Set(attributeName, stringValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error importing attribute '{attributeName}' for key '{key}': {ex.Message}", ex);
                }
            }
            data[key] = attributes;
        }

        ImportKeys(data);
        return this;
    }

    #endregion
}