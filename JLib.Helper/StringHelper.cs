using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace JLib.Helper;

/// <summary>
/// Provides helper methods for string manipulation.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Determines whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the value parameter is null or <see cref="string.Empty"/>, or if value consists exclusively of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhitespace([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Determines whether the specified string is null or an empty string.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
        => string.IsNullOrEmpty(value);

    /// <summary>
    /// Concatenates the specified string the specified number of times.
    /// </summary>
    /// <param name="str">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>A new string that consists of the specified string repeated the specified number of times.</returns>
    public static string Repeat(this string str, int count)
        => new StringBuilder()
            .AppendMultiple(str, count)
            .ToString();

    /// <summary>
    /// removes all substrings, separated by <paramref name="separator"/> from <paramref name="str"/> which match the given <paramref name="predicate"/>
    /// </summary>
    /// <param name="str">the string to perform the operation on</param>
    /// <param name="predicate">The predicate to determine if a line should be removed. The Argument is the current Line</param>
    /// <paramref name="separator">default is <see cref="Environment.NewLine"/></paramref>
    public static string RemoveSubstringsWhere(this string str, Func<string, bool> predicate, string separator)
    {
        return string.Join(separator,
            str.Split(separator).Where(line => predicate(line) is false)
            );
    }

    /// <summary>
    /// removes all substrings, separated by <paramref name="separator"/> from <paramref name="str"/> which match the given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="str">the string to perform the operation on</param>
    /// <param name="predicate">The predicate to determine if a line should be removed. The Arguments are the previous, current and next line respectively.</param>
    /// <param name="separator">default is <see cref="Environment.NewLine"/></param>
    public static string RemoveSubstringsWhere(this string str, Func<string?, string, string?, bool> predicate, string separator)
    {
        var sb = new StringBuilder();
        var substrings = str.Split(separator);
        for (var i = 0; i < substrings.Length; i++)
        {
            var prev = i == 0 ? null : substrings[i - 1];
            var current = substrings[i];
            var next = i == substrings.Length - 1 ? null : substrings[i + 1];

            if (predicate(prev, current, next))
                continue;

            // we have to add the separator before the content, since we don't know if the next lines are removed, which would add a trailing separator in that case
            if (sb.Length > 0)
                sb.Append(separator);
            sb.Append(current);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Appends the specified string to the <see cref="StringBuilder"/> the specified number of times.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="value">The string to append.</param>
    /// <param name="count">The number of times to append the string.</param>
    /// <returns>The <see cref="StringBuilder"/> after the strings have been appended.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the count is less than zero.</exception>
    public static StringBuilder AppendMultiple(this StringBuilder sb, string value, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        for (var i = 0; i < count; i++)
            sb.Append(value);

        return sb;
    }
}
