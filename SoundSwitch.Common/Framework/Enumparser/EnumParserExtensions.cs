#nullable enable
using System;

namespace SoundSwitch.Common.Framework.EnumParser;

public static class EnumParserExtensions
{
    public static T? TryParseEnum<T>(this string? value) where T : struct, Enum
    {
        if (value == null)
            return null;

        if (Enum.TryParse(value, out T result))
        {
            return result;
        }

        return null;
    }
}