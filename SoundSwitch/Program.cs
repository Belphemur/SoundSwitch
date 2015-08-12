using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SoundSwitch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (var mutex = new Mutex(true, "SoundSwitch", out createdNew))
            {
                if (createdNew)
                {
                    if (Properties.Settings.Default.FirstRun)
                    {
                        FirstRun();
                    }
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.ThreadException += Application_ThreadException;
                    new Main();
                    Application.Run();
                }
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var message = String.Format("It seems SoundSwitch has crashed.{0}Do you want to save a log of the error that ocurred?{0}This could be useful to fix bugs. Please post this file in the issues section.", Environment.NewLine);
            var result = MessageBox.Show(message, "SoundSwitch crashed...", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                var textToWrite = String.Format("{0}\nException:\n{1}\n\nInner Exception:\n{2}\n\n\n\n", DateTime.Now, e.Exception.Message, e.Exception.InnerException);
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
                SoundSwitch.Main.Instance.RunAtStartup = true;
            }
            catch (Exception)
            {
                //ignore, for debugging, when the application is not installed as a ClickOnce application.
            }
        }
    }
}
