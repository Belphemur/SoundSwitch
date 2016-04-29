using System.Collections.Generic;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public class DeviceCyclerFactory : AbstractFactory<DeviceCyclerEnumType, IDeviceCycler>
    {
        private static readonly IDictionary<DeviceCyclerEnumType, IDeviceCycler> AllCycler = new Dictionary
            <DeviceCyclerEnumType, IDeviceCycler>
        {
            {DeviceCyclerEnumType.All, new DeviceCyclerAll()},
            {DeviceCyclerEnumType.Available, new DeviceCyclerAvailable()}
        };

        public DeviceCyclerFactory() : base(AllCycler)
        {
        }
    }
}