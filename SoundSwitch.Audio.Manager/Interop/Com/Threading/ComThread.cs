using System;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

namespace SoundSwitch.Audio.Manager.Interop.Com.Threading
{
    internal static class ComThread
    {
        private static bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != Scheduler.ThreadId;

        private static ComTaskScheduler Scheduler { get; } = new ComTaskScheduler();

        /// <summary>
        /// Asserts that the execution following this statement is running on the ComThreads
        /// <exception cref="InvalidThreadException">Thrown if the assertion fails</exception>
        /// </summary>
        public static void Assert()
        {
            if (InvokeRequired)
                throw new InvalidThreadException($"This operation must be run on the ComThread ThreadId: {Scheduler.ThreadId}");
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

        private static Task BeginInvoke(Action action)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Issue while running action in {class}", nameof(ComThread));
                }
            }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
        }

        public static T Invoke<T>(Func<T> func)
        {
            return !InvokeRequired ? func() : BeginInvoke(func).GetAwaiter().GetResult();
        }

        private static Task<T> BeginInvoke<T>(Func<T> func)
        {
            // Exceptions are deliberately NOT caught here: the faulted Task rethrows through
            // Invoke<T>'s GetAwaiter().GetResult(), so the caller sees the real failure instead
            // of a silent default(T) ("no device") result. Issue #2404: converting a transient
            // COM failure into null made devices disappear from the cache until a full restart.
            // The void BeginInvoke(Action) overload stays tolerant on purpose.
            return Task<T>.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, Scheduler);
        }
    }
}
