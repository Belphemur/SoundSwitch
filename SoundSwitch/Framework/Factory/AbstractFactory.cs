using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SoundSwitch.Framework.Factory
{
    /// <summary>
    /// Used to build factory based on Enums
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public abstract class AbstractFactory<TEnum, TImplementation>
    {
        public IReadOnlyDictionary<TEnum, TImplementation> AllImplementations { get; }

        protected AbstractFactory(IDictionary<TEnum, TImplementation> allImplementations)
        {
            AllImplementations = new ReadOnlyDictionary<TEnum, TImplementation>(allImplementations);
        }

        /// <summary>
        /// Get the implementation for the given Enum
        /// </summary>
        /// <param name="eEnum"></param>
        /// <returns></returns>
        public TImplementation Get(TEnum eEnum)
        {
            TImplementation value;
            if (!AllImplementations.TryGetValue(eEnum, out value))
            {
                throw new InvalidEnumArgumentException();
            }
            return value;
        }
    }
}