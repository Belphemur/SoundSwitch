using SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.DeviceCyclerManager
{
    public class DeviceCyclerFactory : AbstractFactory<DeviceCyclerTypeEnum, IDeviceCycler>
    {
        private static readonly IEnumImplList<DeviceCyclerTypeEnum, IDeviceCycler> AllCycler = new EnumImplList
            <DeviceCyclerTypeEnum, IDeviceCycler>
        {
            new DeviceCyclerAll(),
            new DeviceCyclerAvailable()
        };

        public DeviceCyclerFactory() : base(AllCycler)
        {
        }
    }
}