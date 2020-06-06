//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;

namespace SoundSwitch.InterProcess.Communication.Interop
{
    /// <summary>
    /// Native API for Named Pipes
    /// This code was borrowed from PowerShell here:
    /// https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/remoting/common/RemoteSessionNamedPipe.cs#L124-L256
    /// </summary>
    internal static class NamedPipeNative
    {
        #region Pipe constants

        // Pipe open mode
        internal const uint PIPE_ACCESS_DUPLEX = 0x00000003;

        // Pipe modes
        internal const uint PIPE_TYPE_BYTE = 0x00000000;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        internal const uint PIPE_READMODE_BYTE = 0x00000000;

        #endregion

        #region Data structures

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            /// <summary>
            /// The size, in bytes, of this structure. Set this value to the size of the SECURITY_ATTRIBUTES structure.
            /// </summary>
            public int NLength;

            /// <summary>
            /// A pointer to a security descriptor for the object that controls the sharing of it.
            /// </summary>
            public IntPtr LPSecurityDescriptor = IntPtr.Zero;

            /// <summary>
            /// A Boolean value that specifies whether the returned handle is inherited when a new process is created.
            /// </summary>
            public bool InheritHandle;

            /// <summary>
            /// Initializes a new instance of the SECURITY_ATTRIBUTES class
            /// </summary>
            public SECURITY_ATTRIBUTES()
            {
                this.NLength = 12;
            }
        }

        #endregion

        #region Pipe methods

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafePipeHandle CreateNamedPipe(
           string lpName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           SECURITY_ATTRIBUTES securityAttributes);

        internal static SECURITY_ATTRIBUTES GetSecurityAttributes(GCHandle securityDescriptorPinnedHandle, bool inheritHandle = false)
        {
            SECURITY_ATTRIBUTES securityAttributes = new NamedPipeNative.SECURITY_ATTRIBUTES();
            securityAttributes.InheritHandle = inheritHandle;
            securityAttributes.NLength = (int)Marshal.SizeOf(securityAttributes);
            securityAttributes.LPSecurityDescriptor = securityDescriptorPinnedHandle.AddrOfPinnedObject();
            return securityAttributes;
        }

        /// <summary>
        /// Helper method to create a PowerShell transport named pipe via native API, along
        /// with a returned .Net NamedPipeServerStream object wrapping the named pipe.
        /// </summary>
        /// <param name="pipeName">Named pipe core name.</param>
        /// <param name="securityDesc"></param>
        /// <returns>NamedPipeServerStream</returns>
        internal static NamedPipeServerStream CreateNamedPipe(
            string pipeName,
            PipeSecurity pipeSecurity)

        {
            string fullPipeName = @"\\.\pipe\" + pipeName;
            CommonSecurityDescriptor securityDesc = new CommonSecurityDescriptor(false, false, pipeSecurity.GetSecurityDescriptorBinaryForm(), 0);

            // Create optional security attributes based on provided PipeSecurity.
            NamedPipeNative.SECURITY_ATTRIBUTES securityAttributes = null;
            GCHandle? securityDescHandle = null;
            if (securityDesc != null)
            {
                byte[] securityDescBuffer = new byte[securityDesc.BinaryLength];
                securityDesc.GetBinaryForm(securityDescBuffer, 0);

                securityDescHandle = GCHandle.Alloc(securityDescBuffer, GCHandleType.Pinned);
                securityAttributes = NamedPipeNative.GetSecurityAttributes(securityDescHandle.Value);
            }

            // Create named pipe.
            SafePipeHandle pipeHandle = NamedPipeNative.CreateNamedPipe(
                fullPipeName,
                NamedPipeNative.PIPE_ACCESS_DUPLEX | NamedPipeNative.FILE_FLAG_FIRST_PIPE_INSTANCE | NamedPipeNative.FILE_FLAG_OVERLAPPED,
                NamedPipeNative.PIPE_TYPE_BYTE | NamedPipeNative.PIPE_READMODE_BYTE,
                1,
                1024,
                1024,
                0,
                securityAttributes);

            int lastError = Marshal.GetLastWin32Error();
            if (securityDescHandle != null)
            {
                securityDescHandle.Value.Free();
            }

            if (pipeHandle.IsInvalid)
            {
                throw new InvalidOperationException();
            }
            // Create the .Net NamedPipeServerStream wrapper.
            try
            {
                return new NamedPipeServerStream(
                    PipeDirection.InOut,
                    true,                       // IsAsync
                    false,                      // IsConnected
                    pipeHandle);
            }
            catch (Exception)
            {
                pipeHandle.Dispose();
                throw;
            }
        }
        #endregion
    }
}