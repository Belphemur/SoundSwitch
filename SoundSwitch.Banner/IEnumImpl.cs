using System;

namespace SoundSwitch.Banner;

/// <summary>
/// Interface for enum-based implementations, providing a type and a label.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public interface IEnumImpl<out TEnum> where TEnum : Enum, IConvertible
{
    /// <summary>
    /// Gets the enum type.
    /// </summary>
    TEnum TypeEnum { get; }

    /// <summary>
    /// Gets the display label.
    /// </summary>
    string Label { get; }
}
