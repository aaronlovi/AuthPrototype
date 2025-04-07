using System;

namespace AuthPrototype.Utilities;

public static class StringUtils {
    /// <summary>
    /// Truncates the string to the specified threshold if it exceeds the threshold length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="threshold">The maximum length of the string.</param>
    /// <returns>The original string if its length is less than the threshold; otherwise, the truncated string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the string is null.</exception>
    public static string Truncate(this string value, int threshold) {
        ArgumentNullException.ThrowIfNull(value);
        return value.Length <= threshold ? value : value.Substring(0, threshold);
    }
}
