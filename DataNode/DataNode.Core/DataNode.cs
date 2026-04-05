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
public record Properties(Dictionary<string, DnValue> DnValueDictionary)
{
    public static Properties Create() => new([]);

    public string? GetString(string propertyName)
    {
        if (DnValueDictionary.TryGetValue(propertyName, out DnValue? value))
        {
            if (value is StringValue stringValue)
            {
                return stringValue.Value;
            }
        }
        return null;
    }

    public int? GetInteger(string propertyName)
    {
        if (DnValueDictionary.TryGetValue(propertyName, out DnValue? value))
        {
            if (value is IntegerValue integerValue)
            {
                return integerValue.Value;
            }
        }
        return null;
    }

    public decimal? GetDecimal(string propertyName)
    {
        if (DnValueDictionary.TryGetValue(propertyName, out DnValue? value))
        {
            if (value is DecimalValue decimalValue)
            {
                return decimalValue.Value;
            }
        }
        return null;
    }

    public void Set(string propertyName, string value)
    {
        DnValueDictionary[propertyName] = value;
    }

    public void Set(string propertyName, int value)
    {
        DnValueDictionary[propertyName] = value;
    }

    public void Set(string propertyName, decimal value)
    {
        DnValueDictionary[propertyName] = value;
    }

    public void Remove(string propertyName)
    {
        DnValueDictionary.Remove(propertyName);
    }

    public bool Contains(string propertyName)
    {
        return DnValueDictionary.ContainsKey(propertyName);
    }

    public IEnumerable<KeyValuePair<string, DnValue>> AllProperties()
    {
        return DnValueDictionary.Select(kvp => new KeyValuePair<string, DnValue>(kvp.Key, kvp.Value));
    }

    public Properties Copy()
    {
        return new Properties(new Dictionary<string, DnValue>(DnValueDictionary));
    }

    public int Count { get { return DnValueDictionary.Count; } }

}
public record Keys(Dictionary<string, Properties> PropertiesDictionary);
public record Index(List<Properties> PropertiesIndex);

public class DataNode
{
    private readonly Keys Keys = new([]);
    private readonly Index Index = new([]);
    public DataNode() : base()
    {
    }

    public int KeysCount { get { return Keys.PropertiesDictionary.Count ;} }  

    public int IndexCount { get { return Index.PropertiesIndex.Count ;} } 

    public IEnumerable<KeyValuePair<string, Properties>> AllKeys()
    {
        return [.. Keys.PropertiesDictionary.Select(kvp => new KeyValuePair<string, Properties>(kvp.Key, kvp.Value))];
    }

    public IEnumerable<KeyValuePair<int, Properties>> AllIndex()
    {
        return [.. Index.PropertiesIndex.Select((p, i) => new KeyValuePair<int, Properties>(i, p))];
    }
    
    public bool ContainsKey(string key)
    {
        return Keys.PropertiesDictionary.ContainsKey(key);
    }

    public Properties? Get(string key)
    {
        if (Keys.PropertiesDictionary.TryGetValue(key, out Properties? properties))
        {
            return properties;
        }
        else
        {
            return null;
        }
    }

    public Properties GetOrCreate(string key)
    {
        if (Keys.PropertiesDictionary.TryGetValue(key, out Properties? properties))
        {
            return properties;
        }
        else
        {
            Keys.PropertiesDictionary[key] = Properties.Create();
            return Keys.PropertiesDictionary[key];
        }
    }

    public Properties? Get(int index)
    {
        if (index >= 0 && index < Index.PropertiesIndex.Count)
        {
            return Index.PropertiesIndex[index];
        }
        else
        {
            return null;
        }
    }

    public Properties GetOrCreate(int index)
    {
        if (index < 0)
        {
            Index.PropertiesIndex.Insert(0, Properties.Create());
            return Index.PropertiesIndex[0];
        }
        else if (index >= Index.PropertiesIndex.Count)
        {
            Index.PropertiesIndex.Add(Properties.Create());
            return Index.PropertiesIndex[^1];
        }
        else
        {
            return Index.PropertiesIndex[index];
        }
    }

    public Properties GetOrCreate()
    {
        Index.PropertiesIndex.Add(Properties.Create());
        return Index.PropertiesIndex[^1];
    }

    public Properties? GetAndRemove(int index)
    {
        if (index >= 0 && index < Index.PropertiesIndex.Count)
        {
            var properties = Index.PropertiesIndex[index];
            Index.PropertiesIndex.RemoveAt(index);
            return properties;
        }
        else
        {
            return null;
        }
    }

    public void Set(string key, Properties properties)
    {
        Keys.PropertiesDictionary[key] = properties;
    }

    public void Set(int index, Properties properties)
    {
        if (index < 0)
        {
            Index.PropertiesIndex[0] = properties;
        }
        else if (index >= Index.PropertiesIndex.Count)
        {
            Index.PropertiesIndex[^1] = properties;
        }
        else
        {
            Index.PropertiesIndex[index] = properties;
        }
    }

    public void Insert(int index, Properties properties)
    {
        if (index < 0)
        {
            Index.PropertiesIndex.Insert(0, properties);
        }
        else if (index >= Index.PropertiesIndex.Count)
        {
            Index.PropertiesIndex.Add(properties);
        }
        else
        {
            Index.PropertiesIndex.Insert(index, properties);
        }
    }

    public void Add(Properties properties)
    {
        Index.PropertiesIndex.Add(properties);
    }

    public void Remove(string key)
    {
        Keys.PropertiesDictionary.Remove(key);
    }

    public void Remove(int index)
    {
        if (index >= 0 && index < Index.PropertiesIndex.Count)
        {
            Index.PropertiesIndex.RemoveAt(index);
        }
    }
}
