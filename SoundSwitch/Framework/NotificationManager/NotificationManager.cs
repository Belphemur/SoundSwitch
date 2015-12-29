using AudioEndPointControllerWrapper;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationManager
    {
        private readonly IAppModel _model;
        private string _lastDeviceId;

        public NotificationManager(IAppModel model)
        {
            _model = model;
            _model.DefaultDeviceChanged += ModelOnDefaultDeviceChanged;
        }

        private void ModelOnDefaultDeviceChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
        {
            if (_lastDeviceId == deviceDefaultChangedEvent.device.Id)
                return;

            var notification = NotificationFactory.CreateNotification(_model.NotificationSettings, _model.NotifyIcon,
                _model.NotificationSound);
            notification.NotifyDefaultChanged(deviceDefaultChangedEvent.device);
            _lastDeviceId = deviceDefaultChangedEvent.device.Id;
        }

        ~NotificationManager()
        {
            _model.DefaultDeviceChanged -= ModelOnDefaultDeviceChanged;
        }
    }
}