using System;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
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
            SetNotification(_model.NotificationSettings);
        }

        private void ModelOnNotificationSettingsChanged(object sender, NotificationSettingsUpdatedEvent notificationSettingsUpdatedEvent)
        {
            var notificationTypeEnum = notificationSettingsUpdatedEvent.NewSettings;
            SetNotification(notificationTypeEnum);
        }

        private void SetNotification(NotificationTypeEnum notificationTypeEnum)
        {
            try
            {
                _notification = NotificationFactory.CreateNotification(notificationTypeEnum);
            }
            catch (CachedSoundFileNotExistsException)
            {
                MessageBox.Show(string.Format(Properties.Notifications.AudioFileNotFound, Application.ProductName,
                        Properties.Notifications.NotifSound),Properties.Notifications.AudioFileNotFoundTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _model.NotificationSettings = NotificationTypeEnum.SoundNotification;
            }
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