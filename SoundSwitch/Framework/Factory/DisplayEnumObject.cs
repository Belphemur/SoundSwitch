using System;

namespace SoundSwitch.Framework.Factory;

public class DisplayEnumObject<TEnum>(IEnumImpl<TEnum> implementation)
    where TEnum : Enum, IConvertible
{
    /// <summary>
    /// Enum used
    /// </summary>
    public TEnum Enum  {get;} = implementation.TypeEnum;

    /// <summary>
    /// Text to display
    /// </summary>
    public string Display { get; } = implementation.Label;
}