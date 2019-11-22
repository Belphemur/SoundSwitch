using System;

namespace SoundSwitch.Framework.Factory
{
    public class DisplayEnumObject<TEnum> where TEnum : struct, IConvertible
    {
        /// <summary>
        /// Enum used
        /// </summary>
        public TEnum Enum  {get;}

        /// <summary>
        /// Text to display
        /// </summary>
        public string Display { get; }

        public DisplayEnumObject(IEnumImpl<TEnum> implementation)
        {
            Enum = implementation.TypeEnum;
            Display = implementation.Label;
        }
    }
}