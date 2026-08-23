using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// Pure-logic interpretation of <see cref="IPropertyStore.GetValue"/> results.
    /// Kept separate from the COM call so the missing-property contract can be unit-tested.
    /// </summary>
    internal static class PropertyStoreReader
    {
        /// <summary>
        /// Interpret the result of a <see cref="IPropertyStore.GetValue"/> call.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the property is present and its string value was copied out;
        /// <see langword="false"/> when the read failed or the property is absent
        /// (S_OK + VT_EMPTY means "no such property", not an error). When <see langword="false"/>
        /// is returned, <paramref name="value"/> is <see cref="string.Empty"/>.
        /// </returns>
        internal static bool TryReadString(HRESULT hr, ref PropVariant propVariant, out string value)
        {
            value = string.Empty;
            if (hr != HRESULT.S_OK)
            {
                return false;
            }

            if (propVariant.vt == (ushort)VarEnum.VT_EMPTY)
            {
                // Property absent.
                return false;
            }

            if (propVariant.vt != (ushort)VarEnum.VT_LPWSTR)
            {
                return false;
            }

            value = Marshal.PtrToStringUni(propVariant.data1) ?? string.Empty;
            return true;
        }

        /// <summary>
        /// Read a string property from an open property store, honoring the native cleanup contract
        /// (the returned PROPVARIANT is always cleared with PropVariantClear, including the
        /// VT_LPWSTR string heap allocation).
        /// </summary>
        internal static string ReadString(IPropertyStore store, PROPERTYKEY key)
        {
            var propVariant = new PropVariant();
            var hr = store.GetValue(ref key, out propVariant);
            try
            {
                return TryReadString(hr, ref propVariant, out var value) ? value : string.Empty;
            }
            finally
            {
                // Clears the variant no matter the read outcome; safe on VT_EMPTY too.
                PropVariant.PropVariantClear(ref propVariant);
            }
        }
    }
}
