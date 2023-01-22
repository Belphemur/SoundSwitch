namespace SoundSwitch.Util;

public static class StringExtensions
{
    /// <summary>
    /// Truncate a string to the given length
    /// </summary>
    /// <remarks>Only does the truncating if the string is not null or empty and longer than the given <see cref="maxLength"/></remarks>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}