using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SoundSwitch.Forms;

namespace SoundSwitch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (new Mutex(true, Application.ProductName, out createdNew))
            {
                if (!createdNew) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += Application_ThreadException;
                SoundSwitch.Main.InitMain();
                if (Properties.Settings.Default.FirstRun)
                {
                    FirstRun();
                }
                Application.Run();
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var message = string.Format("It seems {1} has crashed.{0}Do you want to save a log of the error that ocurred?{0}This could be useful to fix bugs. Please post this file in the issues section.", Environment.NewLine, Application.ProductName);
            var result = MessageBox.Show(message, $"{Application.ProductName} crashed...", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                var textToWrite =
                    $"{DateTime.Now}\nException:\n{e.Exception.Message}\n\nInner Exception:\n{e.Exception.InnerException}\n\n\n\n";
                var dialog = new SaveFileDialog();
                dialog.ShowDialog();
                using (var sw = new StreamWriter(dialog.OpenFile()))
                {
                    sw.Write(textToWrite);
                }
            }
        }

        private static void FirstRun()
        {
            try
            {
                Settings.Instance.Show();
                Properties.Settings.Default.FirstRun = false;
            }
            catch (Exception)
            {
                //ignore, for debugging, when the application is not installed as a ClickOnce application.
            }
        }
    }
}
