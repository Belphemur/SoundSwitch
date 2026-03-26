using System;

namespace SoundSwitch.Framework.Factory;

public class DisplayEnumObject<TEnum>(IEnumImpl<TEnum> implementation)
    where TEnum : Enum, IConvertible
{
    private readonly IEnumImpl<TEnum> _implementation = implementation;

    /// <summary>
    /// Enum used
    /// </summary>
    public TEnum Enum { get; } = implementation.TypeEnum;

    /// <summary>
    /// Text to display
    /// </summary>
    public string Display => _implementation.Label;
}
