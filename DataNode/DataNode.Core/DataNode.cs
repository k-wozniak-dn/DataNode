using System.Collections.Generic;
namespace DataNode.Core;

public abstract record DnValue
{
    public static implicit operator DnValue(string v)
    {
        return new StringValue(v);
    }

    public static implicit operator DnValue(int v)
    {
        return new IntegerValue(v);
    }

    public static implicit operator DnValue(decimal v)
    {
        return new DecimalValue(v);
    }
}

public record StringValue(string Value) : DnValue;
public record IntegerValue(int Value) : DnValue;
public record DecimalValue(decimal Value) : DnValue;
public record Attributes(Dictionary<string, DnValue> DnValueDictionary, IParentDataNode Parent )
{
    public static Attributes Create(IParentDataNode parent) => new([], parent);

    public string? GetString(string attributeName)
    {
        attributeName = attributeName.ToUpper();
        if (DnValueDictionary.TryGetValue(attributeName, out DnValue? value))
        {
            if (value is StringValue stringValue)
            {
                return stringValue.Value;
            }
        }
        return null;
    }

    public int? GetInteger(string attributeName)
    {
        attributeName = attributeName.ToUpper();
        if (DnValueDictionary.TryGetValue(attributeName, out DnValue? value))
        {
            if (value is IntegerValue integerValue)
            {
                return integerValue.Value;
            }
        }
        return null;
    }

    public decimal? GetDecimal(string attributeName)
    {
        attributeName = attributeName.ToUpper();
        if (DnValueDictionary.TryGetValue(attributeName, out DnValue? value))
        {
            if (value is DecimalValue decimalValue)
            {
                return decimalValue.Value;
            }
        }
        return null;
    }

    public void Set(string attributeName, string value)
    {
        if (attributeName.Length > Parent.AttributeLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {Parent.AttributeLengthLimit} characters.");
        }
        if ( value.Length > Parent.StringValueLengthLimit)
        {
            throw new ArgumentException($"String value length exceeds the limit of {Parent.StringValueLengthLimit} characters.");
        }

        attributeName = attributeName.ToUpper();

        if (DnValueDictionary.Count >= Parent.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {Parent.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Set(string attributeName, int value)
    {
        if (attributeName.Length > Parent.AttributeLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {Parent.AttributeLengthLimit} characters.");
        }

        attributeName = attributeName.ToUpper();

        if (DnValueDictionary.Count >= Parent.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {Parent.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Set(string attributeName, decimal value)
    {
        if (attributeName.Length > Parent.AttributeLengthLimit)
        {
            throw new ArgumentException($"Attribute name length exceeds the limit of {Parent.AttributeLengthLimit} characters.");
        }

        attributeName = attributeName.ToUpper();

        if (DnValueDictionary.Count >= Parent.AttributesCountLimit && !DnValueDictionary.ContainsKey(attributeName))
        {
            throw new InvalidOperationException($"Attributes count exceeds the limit of {Parent.AttributesCountLimit}.");
        }
        DnValueDictionary[attributeName] = value;
    }

    public void Remove(string attributeName)
    {
        attributeName = attributeName.ToUpper();
        DnValueDictionary.Remove(attributeName);
    }

    public bool Contains(string attributeName)
    {
        attributeName = attributeName.ToUpper();
        return DnValueDictionary.ContainsKey(attributeName);
    }

    public IEnumerable<KeyValuePair<string, DnValue>> AllAttributes()
    {
        return DnValueDictionary.Select(kvp => new KeyValuePair<string, DnValue>(kvp.Key, kvp.Value));
    }

    public Attributes Copy(IParentDataNode parent)
    {
        return new Attributes(new Dictionary<string, DnValue>(DnValueDictionary), parent);
    }

    public int Count { get { return DnValueDictionary.Count; } }

}
public record Keys(Dictionary<string, Attributes> AttributesDictionary);
public record Index(List<Attributes> AttributesIndex);

public interface IParentDataNode
{
    short AttributeLengthLimit { get; }
    short StringValueLengthLimit { get; }
    short AttributesCountLimit { get; }

    short KeyLengthLimit { get; }
    short KeysCountLimit { get; }
    short IndexCountLimit { get; }
}

public class DataNode : IParentDataNode
{
    #region Properties and Fields
    private readonly Keys Keys = new([]);
    private readonly Index Index = new([]);
    public short AttributeLengthLimit { get { return 64; } }
    public short AttributesCountLimit { get { return 8; } }
    public short StringValueLengthLimit { get { return 256; } }    
    public short KeyLengthLimit { get { return 64; } }
    public short KeysCountLimit { get { return 72; } }
    public short IndexCountLimit { get { return 64; } }
    public int KeysCount { get { return Keys.AttributesDictionary.Count ;} }  
    public int IndexCount { get { return Index.AttributesIndex.Count ;} } 

    #endregion

    public DataNode() : base()
    {
    }

    #region Keys
    public IEnumerable<KeyValuePair<string, Attributes>> AllKeys()
    {
        return [.. Keys.AttributesDictionary.Select(kvp => new KeyValuePair<string, Attributes>(kvp.Key, kvp.Value))];
    }
    
    public bool ContainsKey(string key)
    {
        key = key.ToUpper();
        return Keys.AttributesDictionary.ContainsKey(key);
    }

    public void Set(string key, Attributes properties)
    {
        key = key.ToUpper();

        if (key.Length > KeyLengthLimit)
        {
            throw new ArgumentException($"Key length exceeds the limit of {KeyLengthLimit} characters.");
        }
        if (Keys.AttributesDictionary.Count >= KeysCountLimit && !ContainsKey(key))
        {
            throw new InvalidOperationException($"Keys count exceeds the limit of {KeysCountLimit}.");
        }
        Keys.AttributesDictionary[key] = properties;
    }

    public Attributes? Get(string key)
    {
        key = key.ToUpper();

        if (Keys.AttributesDictionary.TryGetValue(key, out Attributes? properties))
        {
            return properties;
        }
        else
        {
            return null;
        }
    }

    public Attributes GetOrCreate(string key)
    {
        key = key.ToUpper();

        var properties = Get(key);
        if (properties != null)
        {
            return properties;
        }
        else
        {
            properties = Attributes.Create(this);
            Set(key, properties);
            return properties;
        }
    }

    public void Remove(string key)
    {
        key = key.ToUpper();
        Keys.AttributesDictionary.Remove(key);
    }

    #endregion

    #region Index
    public IEnumerable<KeyValuePair<int, Attributes>> AllIndex()
    {
        return [.. Index.AttributesIndex.Select((p, i) => new KeyValuePair<int, Attributes>(i, p))];
    }
    
    public void Add(Attributes properties)
    {
        if (Index.AttributesIndex.Count >= IndexCountLimit)
        {
            throw new InvalidOperationException($"Index count exceeds the limit of {IndexCountLimit}.");
        }
        Index.AttributesIndex.Add(properties);
    }

    public Attributes Add()
    {
        Add(Attributes.Create(this));
        return Index.AttributesIndex[^1];
    }

    public void Set(int index, Attributes properties)
    {
        if (index < 0)
        {
            Index.AttributesIndex[0] = properties;
        }
        else if (index >= Index.AttributesIndex.Count)
        {
            Index.AttributesIndex[^1] = properties;
        }
        else
        {
            Index.AttributesIndex[index] = properties;
        }
    }

    public void Insert(int index, Attributes properties)
    {
        if (Index.AttributesIndex.Count >= IndexCountLimit)
        {
            throw new InvalidOperationException($"Index count exceeds the limit of {IndexCountLimit}.");
        }

        if (index < 0)
        {
            Index.AttributesIndex.Insert(0, properties);
        }
        else if (index >= Index.AttributesIndex.Count)
        {
            Add(properties);
        }
        else
        {
            Index.AttributesIndex.Insert(index, properties);
        }
    }

    public Attributes? Get(int index)
    {
        if (index >= 0 && index < Index.AttributesIndex.Count)
        {
            return Index.AttributesIndex[index];
        }
        else
        {
            return null;
        }
    }

    public Attributes GetOrCreate(int index)
    {
        var properties = Get(index);
        if (properties != null)
        {
            return properties;
        }
        else
        {
            properties = Attributes.Create(this);
            Insert(index, properties);
            return properties;
        }
    }

    public Attributes? GetAndRemove(int index)
    {
        if (index >= 0 && index < Index.AttributesIndex.Count)
        {
            var properties = Index.AttributesIndex[index];
            Index.AttributesIndex.RemoveAt(index);
            return properties;
        }
        else
        {
            return null;
        }
    }

    public void Remove(int index)
    {
        if (index >= 0 && index < Index.AttributesIndex.Count)
        {
            Index.AttributesIndex.RemoveAt(index);
        }
    }    

    #endregion

}
