using System;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationManager
    {
        private readonly IAppModel _model;
        private string _lastDeviceId;
        private INotification _notification;

        public NotificationManager(IAppModel model)
        {
            _model = model;
        }

        public void Init()
        {
            _model.DefaultDeviceChanged += ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged += ModelOnNotificationSettingsChanged;
            _notification = NotificationFactory.CreateNotification(_model.NotificationSettings, _model.NotifyIcon,
                _model.NotificationSound);
        }

        private void ModelOnNotificationSettingsChanged(object sender, NotificationSettingsUpdatedEvent notificationSettingsUpdatedEvent)
        {
            _notification = NotificationFactory.CreateNotification(notificationSettingsUpdatedEvent.NewSettings, _model.NotifyIcon,
               _model.NotificationSound);
        }

        private void ModelOnDefaultDeviceChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
        {
            if (_lastDeviceId == deviceDefaultChangedEvent.device.Id)
                return;

           
            _notification.NotifyDefaultChanged(deviceDefaultChangedEvent.device);
            _lastDeviceId = deviceDefaultChangedEvent.device.Id;
        }

        ~NotificationManager()
        {
            _model.DefaultDeviceChanged -= ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged -= ModelOnNotificationSettingsChanged;
        }
    }
}