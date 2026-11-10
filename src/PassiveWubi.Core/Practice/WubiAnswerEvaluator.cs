namespace PassiveWubi.Core.Practice;

public static class WubiAnswerEvaluator
{
    public static string Normalize(string? answer) =>
        (answer ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsCorrect(string expected, string? answer) =>
        string.Equals(
            Normalize(expected),
            Normalize(answer),
            StringComparison.Ordinal);

    public static string DescribeCode(WubiVersion version, string code)
    {
        var layout = WubiDefinitions.GetLayout(version)
            .ToDictionary(item => item.Key, StringComparer.OrdinalIgnoreCase);
        var parts = new List<string>();
        foreach (var letter in Normalize(code))
        {
            if (layout.TryGetValue(letter.ToString(), out var definition))
            {
                parts.Add($"{char.ToUpperInvariant(letter)} {definition.KeyName}");
            }
        }

        return string.Join(" · ", parts);
    }
}
