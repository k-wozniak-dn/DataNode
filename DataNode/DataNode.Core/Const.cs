namespace DataNode.Core;

public static class System
{
    public const string SysPrefix = "*";
    public const short AttributeNameLengthLimit = 64;
    public const short AttributesCountLimit = 8;
    public const short StringValueLengthLimit = 256;   
    public const short KeyLengthLimit = 64;
    public const short KeysCountLimit = 136;
}

public static class SysKeys
{
    public const string Id = $"{System.SysPrefix}ID";
    public const string NoIndex = $"{System.SysPrefix}NO-INDEX-KEYS";

    public static readonly HashSet<string> All = [Id, NoIndex];
}