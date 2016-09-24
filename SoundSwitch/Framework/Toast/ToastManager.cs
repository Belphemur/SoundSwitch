using Windows.UI.Notifications;

namespace SoundSwitch.Framework.Toast
{
    public class ToastManager
    {
        private const string APP_ID = "aaflalo.SoundSwitch.Application";

        /// <summary>
        /// Show a toast notification with the given data
        /// </summary>
        /// <param name="data"></param>
        public void ShowNotification(ToastData data)
        {
            var notification = data.BuildNotification();
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(notification);
        }
    }
}