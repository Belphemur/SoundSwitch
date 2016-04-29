using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Factory
{
    /// <summary>
    ///     Used to build factory based on Enums
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public abstract class AbstractFactory<TEnum, TImplementation> where TImplementation : IEnumImpl<TEnum>
        where TEnum : struct, IConvertible
    {
        protected AbstractFactory(IDictionary<TEnum, TImplementation> allImplementations)
        {
            AllImplementations = new ReadOnlyDictionary<TEnum, TImplementation>(allImplementations);
        }

        public IReadOnlyDictionary<TEnum, TImplementation> AllImplementations { get; }

        /// <summary>
        ///     Get the implementation for the given Enum
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

        /// <summary>
        ///     Configure the list control DataSource, ValueMember and DisplayMember
        /// </summary>
        /// <param name="list"></param>
        public void ConfigureListControl(ListControl list)
        {
            list.DataSource =
                AllImplementations.Values.Select(
                    implementation => new {Type = implementation.TypeEnum, Display = implementation.Label})
                    .ToArray();
            list.ValueMember = "Type";
            list.DisplayMember = "Display";
        }
    }
}