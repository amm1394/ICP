namespace Shared.Icp.Helpers.Extensions
{
    /// <summary>
    /// Extension methods that provide common helpers for working with <see cref="string"/> values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These helpers are null-safe where applicable and avoid throwing exceptions for common checks and conversions.
    /// They do not mutate the input values.
    /// </para>
    /// <para>
    /// Localization: keep any user-facing Persian texts outside of these helpers. Parsing methods use the default
    /// framework behavior and do not enforce a specific culture unless noted.
    /// </para>
    /// </remarks>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the specified string is <c>null</c> or an empty string; otherwise, <c>false</c>.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="value"/> is <c>null</c> or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Returns <c>true</c> if the specified string is <c>null</c>, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="value"/> is <c>null</c>, empty, or whitespace; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Trims leading and trailing white-space characters from the string in a null-safe manner.
        /// </summary>
        /// <param name="value">The string to trim; may be <c>null</c>.</param>
        /// <returns>
        /// The trimmed string, or <c>null</c> when <paramref name="value"/> is <c>null</c>.
        /// </returns>
        public static string? SafeTrim(this string? value)
        {
            return value?.Trim();
        }

        /// <summary>
        /// Attempts to convert the string to a <see cref="decimal"/> value; returns <c>null</c> when conversion fails.
        /// </summary>
        /// <param name="value">The input string to parse. When <c>null</c> or whitespace, returns <c>null</c>.</param>
        /// <returns>
        /// The parsed <see cref="decimal"/> value, or <c>null</c> if parsing fails.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="decimal.TryParse(string?, out decimal)"/> with the current culture by default.
        /// Ensure the decimal separator in the input aligns with the active culture settings.
        /// </remarks>
        public static decimal? ToDecimalOrNull(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Attempts to convert the string to a 32-bit integer; returns <c>null</c> when conversion fails.
        /// </summary>
        /// <param name="value">The input string to parse. When <c>null</c> or whitespace, returns <c>null</c>.</param>
        /// <returns>
        /// The parsed <see cref="int"/> value, or <c>null</c> if parsing fails.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="int.TryParse(string?, out int)"/> with the current culture by default.
        /// </remarks>
        public static int? ToIntOrNull(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Returns a new string with only letters, digits, and whitespace retained; removes other characters.
        /// </summary>
        /// <param name="value">The input string to filter. If <c>null</c> or empty, the original value is returned.</param>
        /// <returns>
        /// A filtered string containing only alphanumeric and whitespace characters.
        /// </returns>
        /// <remarks>
        /// This method keeps characters for which <see cref="char.IsLetterOrDigit(char)"/> or <see cref="char.IsWhiteSpace(char)"/>
        /// returns <c>true</c>. Punctuation and symbols are removed. Be mindful that some locale-specific letters and digits
        /// are supported by <c>IsLetterOrDigit</c>, but certain Unicode categories may still be stripped.
        /// </remarks>
        public static string RemoveSpecialCharacters(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return new string(value.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        }

        /// <summary>
        /// Returns a string truncated to the specified maximum length.
        /// </summary>
        /// <param name="value">The input string to truncate. If <c>null</c> or empty, the original value is returned.</param>
        /// <param name="maxLength">The maximum allowed length. Must be greater than or equal to 0.</param>
        /// <returns>
        /// The original string if its length is less than or equal to <paramref name="maxLength"/>; otherwise, the
        /// substring starting at index 0 with length <paramref name="maxLength"/>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="maxLength"/> is less than 0, the underlying <see cref="string.Substring(int, int)"/>
        /// call would throw. Callers should ensure a non-negative maximum.
        /// </remarks>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}