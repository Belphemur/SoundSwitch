using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundSwitch.Audio.Manager.Interop.Threading
{
    internal static class ComThread
    {
        private static readonly ComTaskScheduler COM_SCHEDULER = new ComTaskScheduler();

        private static bool InvokeRequired
        {
            get { return Thread.CurrentThread.ManagedThreadId != Scheduler.ThreadId; }
        }

        private static ComTaskScheduler Scheduler
        {
            get { return COM_SCHEDULER; }
        }

        /// <summary>
        /// Asserts that the execution following this statement is running on the ComThreads
        /// <exception cref="InvalidThreadException">Thrown if the assertion fails</exception>
        /// </summary>
        public static void Assert()
        {
            if (InvokeRequired)
                throw new InvalidThreadException(String.Format("This operation must be run on the ComThread ThreadId: {0}", Scheduler.ThreadId));
        }

        public static void Invoke(Action action)
        {
            if (!InvokeRequired)
            {
                action();
                return;
            }

            BeginInvoke(action).Wait();
        }

        public static Task BeginInvoke(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, COM_SCHEDULER);
        }

        public static T Invoke<T>(Func<T> func)
        {
            if (!InvokeRequired)
                return func();

            return BeginInvoke(func).Result;
        }

        public static Task<T> BeginInvoke<T>(Func<T> func)
        {
            return Task<T>.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, COM_SCHEDULER);
        }
    }
}