namespace DataNode.Core;

public static class System
{
    public const short AttributeNameLengthLimit = 64;
    public const short AttributesCountLimit = 16;
    public const short StringValueLengthLimit = 256;
    public const short KeyLengthLimit = 64;
    public const short ItemsCountLimit = 64;
    public const string SysKeyPrefix = "*";
    public const string SysAttributeNamePrefix = "*"; 
}

public static class SystemKeys
{
    public const string Header = $"{System.SysKeyPrefix}ID";
    public static readonly HashSet<string> All = [Header];
}

public static class SystemAttributes
{
    public const string Position = $"{System.SysAttributeNamePrefix}POS";
    public const string Unindexed = $"{System.SysAttributeNamePrefix}UNINDEXED";
    public static readonly HashSet<string> All = [Position, Unindexed];
}