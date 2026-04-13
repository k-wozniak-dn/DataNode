
namespace DataNode.Core;

public interface IParentDataNode
{
}

public class DataNode : IParentDataNode
{
    public DataNode() : base()
    {
    }

    #region Properties and Fields
    private readonly Dictionary<string, Attributes> Keys = new([]);
    private readonly List<string> Index = new([]);
    public int CountNonSys { get { return Keys.Count(kvp => !kvp.Key.StartsWith(System.SysPrefix)) ;} }  

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
   
    public IEnumerable<KeyValuePair<string, Attributes>> GetAll()
    {
        var sys = Keys
        .Where(kvp => kvp.Key.StartsWith(System.SysPrefix))
        .OrderBy(kvp => kvp.Key);

        var nonIndexed = Keys
        .Where(kvp => !Index.Contains(kvp.Key) && !kvp.Key.StartsWith(System.SysPrefix))
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

    public void Set(string key, Attributes attributes)
    {
        key = ValidateKey(key);

        if (key.Length > System.KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {System.KeyLengthLimit} characters.");
        }
        if (CountNonSys >= System.KeysCountLimit && !ContainsKey(key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {System.KeysCountLimit}.");
        }
        attributes.Key = key;
        attributes.Parent = this;
        Keys[key] = attributes;
        SetIndex(key);
    }

    public Attributes? Get(string key)
    {
        key = ValidateKey(key);

        if (Keys.TryGetValue(key, out Attributes? attributes))
        {
            return attributes;
        }
        else
        {
            return null;
        }
    }

    public Attributes GetOrCreate(string key)
    {
        key = ValidateKey(key);

        var attributes = Get(key);
        if (attributes != null)
        {
            return attributes;
        }
        else
        {
            attributes = Attributes.Create(this, key);
            Set(key, attributes);
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

    public void Set(string key, string attributeName, string value)
    {
        var attributes = GetOrCreate(key);
        attributes.Set(attributeName, value);
    }

    public void Set(string key, string attributeName, int value)
    {
        var attributes = GetOrCreate(key);
        attributes.Set(attributeName, value);
    }

    public void Set(string key, string attributeName, decimal value)
    {
        var attributes = GetOrCreate(key);
        attributes.Set(attributeName, value);
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
    public int SetIndex(string key)
    {
        key = ValidateKey(key);

        if (key.StartsWith(System.SysPrefix))
        {
            return -1; // System keys are not indexed
        }

        if (Contains(SysKeys.NoIndex, key))
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

    public void SetUnindexed(string key)
    {
        Set(SysKeys.NoIndex, key, 1);
        RemoveIndex(key);
    }

    public int SetIndexed(string key)
    {
        Remove(SysKeys.NoIndex, key);
        return SetIndex(key);
    }

    public void SetIndexAttributes()
    {
        Index.ForEach(key => Set(key, SysAttributes.Position, Index.IndexOf(key)));
    }

    public void ClearIndexAttributes()
    {
        GetAll().Where(kvp => kvp.Value.Contains(SysAttributes.Position)).ToList()
        .ForEach(kvp => Remove(kvp.Key, SysAttributes.Position));
    }
    #endregion

    #region Export and Import
    public IEnumerable<KeyValuePair<string, Attributes>> Export()
    {
        SetIndexAttributes();
        var result = GetAll();
        ClearIndexAttributes();
        return result;
    }

    public void Import(IEnumerable<KeyValuePair<string, Attributes>> data)
    {
        Clear();

        var sys = data
        .Where(kvp => kvp.Key.StartsWith(System.SysPrefix))
        .OrderBy(kvp => kvp.Key);

        var nonIndexed = data
        .Where(kvp => !kvp.Key.StartsWith(System.SysPrefix) && !kvp.Value.Contains(SysAttributes.Position))
        .OrderBy(kvp => kvp.Key);

        var indexed = data
        .Where(kvp => !kvp.Key.StartsWith(System.SysPrefix) && kvp.Value.Contains(SysAttributes.Position))
        .OrderBy(kvp => kvp.Value.GetInteger(SysAttributes.Position));

        var import = sys.Concat(nonIndexed).Concat(indexed).ToArray();

        foreach (var kvp in import)
        {
            var attributes = kvp.Value.Copy(this, kvp.Key);
            Set(kvp.Key, attributes);
        }

        ClearIndexAttributes();
    }
    #endregion
}