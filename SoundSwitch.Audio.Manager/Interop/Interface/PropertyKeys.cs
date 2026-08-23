using System;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// The property keys SoundSwitch reads or filters on. Values match the Windows SDK definitions
    /// (and the previously used library constants) exactly.
    /// </summary>
    public static class PropertyKeys
    {
        /// <summary>{026e516e-b814-414b-83cd-856d6fef4822}, pid 2</summary>
        public static readonly PROPERTYKEY PKEY_DeviceInterface_FriendlyName = new(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 2);

        /// <summary>{a45c254e-df1c-4efd-8020-67d146a850e0}, pid 14</summary>
        public static readonly PROPERTYKEY PKEY_Device_FriendlyName = new(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);

        /// <summary>{259abffc-50a7-47ce-af08-68c9a7d73366}, pid 12</summary>
        public static readonly PROPERTYKEY PKEY_Device_IconPath = new(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 12);

        /// <summary>{1da5d803-d492-4edd-8c23-e0c0ffee7f0e}, pid 4</summary>
        public static readonly PROPERTYKEY PKEY_AudioEndpoint_GUID = new(new Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), 4);

        /// <summary>{a45c254e-df1c-4efd-8020-67d146a850e0}, pid 24</summary>
        public static readonly PROPERTYKEY DEVPKEY_Device_EnumeratorName = new(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 24);
    }
}
