using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoundSwitch.Audio.Manager.Interop.Com.Threading
{
    internal sealed class ComTaskScheduler : TaskScheduler, IDisposable
    {
        /// <summary>The STA threads used by the scheduler.</summary>
        private readonly Thread _thread;

        /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
        private BlockingCollection<Task> _tasks;

        /// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
        public ComTaskScheduler()
        {
            // Initialize the tasks collection
            _tasks = new BlockingCollection<Task>();

            _thread = new Thread(() =>
            {
                // Continually get the next task and try to execute it.
                // This will continue until the scheduler is disposed and no more tasks remain.
                foreach (var t in _tasks.GetConsumingEnumerable())
                {
                    TryExecuteTask(t);
                }

                //lightweight pump of the thread
                Thread.CurrentThread.Join(1);
            }) {Name = "ComThread", IsBackground = true};

            _thread.SetApartmentState(ApartmentState.STA);

            // Start all of the threads
            _thread.Start();
        }

        public int ThreadId => _thread?.ManagedThreadId ?? -1;

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel => 1;

        /// <summary>
        ///     Cleans up the scheduler by indicating that no more tasks will be queued.
        ///     This method blocks until all threads successfully shutdown.
        /// </summary>
        public void Dispose()
        {
            if (_tasks == null) return;

            // Indicate that no new tasks will be coming in
            _tasks.CompleteAdding();

            _thread.Join();

            // Cleanup
            _tasks.Dispose();
            _tasks = null;
        }

        /// <summary>Queues a Task to be executed by this scheduler.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            // Push it into the blocking collection of tasks
            _tasks.Add(task);
        }

        /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
        /// <returns>An enumerable of all tasks currently scheduled.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            // Serialize the contents of the blocking collection of tasks for the debugger
            return _tasks.ToArray();
        }

        /// <summary>Determines whether a Task may be inlined.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            //Never run inline, it HAS to be run on the COM thread
            return false;
        }
    }
}