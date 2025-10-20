using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Cve : ISpanParsable<Cve>
{
    [JsonPropertyName("cve-id")]
    public required string Id { get; set; }

    [JsonPropertyName("cve-url")]
    public required Uri Url { get; set; }

    public static Cve Parse(string s, IFormatProvider? provider)
    {
        return !TryParse(s.AsSpan(), provider, out var result)
            ? throw new FormatException("Input string was not in the correct format for a CVE.")
            : result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Cve result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    public static Cve Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return !TryParse(s, provider, out var result)
            ? throw new FormatException("Input string was not in the correct format for a CVE.")
            : result;
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Cve result)
    {
        result = null;

        if (s.Length < 9 || !s.StartsWith("CVE-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var firstHyphenIndex = s.IndexOf('-');
        var secondHyphenIndex = s[(firstHyphenIndex + 1)..].IndexOf('-');

        if (secondHyphenIndex == -1)
        {
            return false;
        }

        var yearEndIndex = firstHyphenIndex + 1 + secondHyphenIndex;
        var yearSpan = s[4..yearEndIndex];
        var numberSpan = s[(yearEndIndex + 1)..];

        if (!IsAllDigits(yearSpan) || !IsAllDigits(numberSpan))
        {
            return false;
        }

        var id = s.ToString();

        if (!Uri.TryCreate($"https://ubuntu.com/security/{id}", UriKind.Absolute, out var url))
        {
            return false;
        }

        result = new Cve { Id = id, Url = url };
        return true;
    }

    private static bool IsAllDigits(ReadOnlySpan<char> s)
    {
        foreach (var c in s)
        {
            if (!char.IsAsciiDigit(c))
            {
                return false;
            }
        }
        return true;
    }
}
