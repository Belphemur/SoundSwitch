//Source: https://github.com/danielsunnerberg/AudioDevice-Quickswitcher

using System;
using System.Diagnostics;

namespace SoundSwitch.Util
{
    /// <summary>
    /// Provides methods to interact with a process.
    /// Includes starting/querying it and processing its output.
    /// </summary>
    class ProcessExecutor
    {

        private Process _process;

        /// <summary>
        /// Creates a new instance using the specified path.
        /// </summary>
        /// <param name="processPath">Path to the desired process which will be interacted with</param>
        public ProcessExecutor(string processPath)
        {
            SetupProcess(processPath);
        }

        private void SetupProcess(string processPath)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
        }

        /// <summary>
        /// Starts the process with the specified arguments.
        /// </summary>
        /// <param name="arguments">Arguments, if any, which the process should be started with.</param>
        public bool Start(string arguments)
        {
            _process.StartInfo.Arguments = arguments;
            _process.Start();
            if (!_process.WaitForExit(5000))
            {
                throw new TimeoutException("Timed out while trying to switch audio output.");
            }
            return (_process.ExitCode == 0);
        }

        /// <summary>
        /// Starts the process with the specified arguments, returning any output.
        /// </summary>
        /// <param name="arguments">Arguments, if any, which the process should be started with.</param>
        /// <returns>Eventual output from started process</returns>
        public string Query(string arguments)
        {
            Start(arguments);
            return _process.StandardOutput.ReadToEnd();
        }

    }
}
