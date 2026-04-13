namespace DataNode.Core;

public static class System
{
    public const string SysPrefix = "*";
    public const short AttributeNameLengthLimit = 64;
    public const short AttributesCountLimit = 16;
    public const short StringValueLengthLimit = 256;
    public const short KeyLengthLimit = 64;
    public const short KeysCountLimit = 64;
}

public static class SysKeys
{
    public const string Id = $"{System.SysPrefix}ID";
    public const string NoIndex = $"{System.SysPrefix}NO-INDEX-KEYS";

    public static readonly HashSet<string> All = [Id, NoIndex];
}

public static class SysAttributes
{
    public const string Position = $"{System.SysPrefix}POS";

    public static readonly HashSet<string> All = [Position];
}