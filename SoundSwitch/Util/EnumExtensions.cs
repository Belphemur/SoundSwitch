using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundSwitch.Util;

public static class EnumExtensions
{
    /// <summary>
    /// Get the unique flag in the Flag Enum
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<T> GetUniqueFlags<T>(this T flags) where T : Enum
    {
        ulong flag = 1;
        foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
        {
            ulong bits = Convert.ToUInt64(value);
            while (flag < bits)
            {
                flag <<= 1;
            }

            if (flag == bits && flags.HasFlag(value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Gets the attribute of type T for an enum value
    /// </summary>
    public static T GetAttribute<T>(this Enum value) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null) return null;

        return (T)Attribute.GetCustomAttribute(fieldInfo, typeof(T));
    }
}