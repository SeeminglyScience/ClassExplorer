using System;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace ClassExplorer;

internal sealed class StringMatcher
{
    private readonly unsafe delegate*<string?, string, WildcardPattern, bool> _matcher;

    private readonly string _expected;

    private readonly WildcardPattern _pattern;

    private unsafe StringMatcher(
        delegate*<string?, string, WildcardPattern, bool> matcher,
        string expected,
        WildcardPattern pattern)
    {
        _matcher = matcher;
        _expected = expected;
        _pattern = pattern;
    }

    public static unsafe Regex CreateRegex(string pattern)
    {
        RegexOptions options = HasUppercaseCharacter(pattern.AsSpan())
            ? RegexOptions.None
            : RegexOptions.IgnoreCase;

        return new Regex(pattern, options);
    }

    public static unsafe StringMatcher Create(string input)
    {
        if (WildcardPattern.ContainsWildcardCharacters(input))
        {
            WildcardOptions options = HasUppercaseCharacter(input.AsSpan())
                ? WildcardOptions.None
                : WildcardOptions.IgnoreCase;

            return new(&Pattern, null!, new WildcardPattern(input, options));
        }

        for (int i = input.Length - 1; i >= 0; i--)
        {
            if (char.IsUpper(input[i]))
            {
                return new(&Ordinal, input, null!);
            }
        }

        return new(&IgnoreCase, input, null!);
    }

    public unsafe bool IsMatch(string? input)
    {
        // There's a solid chance this isn't actually faster than just branching
        // here, I'll test one day.
        return _matcher(input, _expected, _pattern);
    }

    private static bool Pattern(string? actual, string _, WildcardPattern pattern)
    {
        return actual is not null && pattern.IsMatch(actual);
    }

    private static bool Ordinal(string? actual, string expected, WildcardPattern _)
    {
        return actual is not null && expected.IsExactly(actual);
    }

    private static bool IgnoreCase(string? actual, string expected, WildcardPattern _)
    {
        return actual is not null && expected.Equals(actual, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasUppercaseCharacter(ReadOnlySpan<char> value)
    {
        for (int i = value.Length - 1; i >= 0; i--)
        {
            if (char.IsUpper(value[i]))
            {
                return true;
            }
        }

        return false;
    }
}
