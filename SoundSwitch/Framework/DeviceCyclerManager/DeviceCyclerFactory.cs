using System.Collections.Generic;
using SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.DeviceCyclerManager
{
    public class DeviceCyclerFactory : AbstractFactory<DeviceCyclerTypeEnum, IDeviceCycler>
    {
        private static readonly IDictionary<DeviceCyclerTypeEnum, IDeviceCycler> AllCycler = new Dictionary
            <DeviceCyclerTypeEnum, IDeviceCycler>
        {
            {DeviceCyclerTypeEnum.All, new DeviceCyclerAll()},
            {DeviceCyclerTypeEnum.Available, new DeviceCyclerAvailable()}
        };

        public DeviceCyclerFactory() : base(AllCycler)
        {
        }
    }
}