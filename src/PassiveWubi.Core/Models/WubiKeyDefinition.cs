namespace PassiveWubi.Core;

public sealed class WubiKeyDefinition
{
    public required string Key { get; init; }
    public required string KeyName { get; init; }
    public string SimpleCode { get; init; } = string.Empty;
    public IReadOnlyList<string> Roots { get; init; } = [];
    public required string AccentColor { get; init; }
    public string Zone { get; init; } = string.Empty;
}
