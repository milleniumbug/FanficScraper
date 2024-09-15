using System.Diagnostics.CodeAnalysis;

namespace Common.Utils;

public static class StringUtils
{
    public static bool TryTrimStart(
        this string s,
        string trimString,
        StringComparison comparisonType,
        [NotNullWhen(true)] out string? result)
    {
        if (s.StartsWith(trimString, comparisonType))
        {
            result = s[trimString.Length..];
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }
}