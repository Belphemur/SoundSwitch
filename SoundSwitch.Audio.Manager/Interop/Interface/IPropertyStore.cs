using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IPropertyStore (propsys.h). Vtable order matches the Windows SDK declaration exactly.
    /// Only reads are used; SetValue/Commit are inert vtable slots.
    /// </summary>
    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        HRESULT GetCount([Out] out uint propCount);

        [PreserveSig]
        HRESULT GetAt([In] uint property, [Out] out PROPERTYKEY key);

        /// <summary>
        /// Reads a property value. The returned <see cref="PropVariant"/> must be cleared with
        /// PropVariantClear once the value has been copied out — including string-valued
        /// VT_LPWSTR variants, whose pwszVal heap allocation is freed by the clear.
        /// S_OK with VT_EMPTY means the property is absent (not an error).
        /// </summary>
        [PreserveSig]
        HRESULT GetValue([In] ref PROPERTYKEY key, [Out] out PropVariant value);

        [PreserveSig]
        HRESULT SetValue([In] ref PROPERTYKEY key, [In] ref PropVariant value);

        [PreserveSig]
        HRESULT Commit();
    }
}
