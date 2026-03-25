using SoundSwitch.Common.Framework.Factory;
using System.Globalization;

namespace SoundSwitch.Localization.Factory.Lang;

public interface ILang : IEnumImpl<Language>
{
    /// <summary>
    /// Culture info of this language
    /// </summary>
    CultureInfo CultureInfo { get; }

    /// <summary>
    /// Is this language read from Right to left
    /// </summary>
    bool IsRightToLeft { get; }
}
