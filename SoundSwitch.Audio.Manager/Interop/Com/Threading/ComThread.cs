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
            return Task<T>.Factory.StartNew(() =>
            {
                try
                {
                    return func();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Issue while running func in {class}", nameof(ComThread));
                    return default;
                }
               
            }, CancellationToken.None, TaskCreationOptions.None, Scheduler);
        }
    }
}