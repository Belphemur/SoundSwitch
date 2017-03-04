/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SoundSwitch.Framework.Minidump
{
    public static class MiniDump

    {
        public enum ExceptionInfo

        {
            None,

            Present
        }

        // Taken almost verbatim from http://blog.kalmbach-software.de/2008/12/13/writing-minidumps-in-c/

        [Flags]
        public enum Option : uint

        {
            // From dbghelp.h:

            Normal = 0x00000000,

            WithDataSegs = 0x00000001,

            WithFullMemory = 0x00000002,

            WithHandleData = 0x00000004,

            FilterMemory = 0x00000008,

            ScanMemory = 0x00000010,

            WithUnloadedModules = 0x00000020,

            WithIndirectlyReferencedMemory = 0x00000040,

            FilterModulePaths = 0x00000080,

            WithProcessThreadData = 0x00000100,

            WithPrivateReadWriteMemory = 0x00000200,

            WithoutOptionalData = 0x00000400,

            WithFullMemoryInfo = 0x00000800,

            WithThreadInfo = 0x00001000,

            WithCodeSegs = 0x00002000,

            WithoutAuxiliaryState = 0x00004000,

            WithFullAuxiliaryState = 0x00008000,

            WithPrivateWriteCopyMemory = 0x00010000,

            IgnoreInaccessibleMemory = 0x00020000,

            ValidTypeFlags = 0x0003ffff
        }


        //BOOL

        //WINAPI

        //MiniDumpWriteDump(

        //    __in HANDLE hProcess,

        //    __in DWORD ProcessId,

        //    __in HANDLE hFile,

        //    __in MINIDUMP_TYPE DumpType,

        //    __in_opt PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,

        //    __in_opt PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,

        //    __in_opt PMINIDUMP_CALLBACK_INFORMATION CallbackParam

        //    );


        // Overload requiring MiniDumpExceptionInformation

        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType,
            ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);


        // Overload supporting MiniDumpExceptionInformation == NULL

        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType,
            IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);


        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        private static extern uint GetCurrentThreadId();


        public static bool Write(SafeHandle fileHandle, Option options, ExceptionInfo exceptionInfo)

        {
            var currentProcess = Process.GetCurrentProcess();

            var currentProcessHandle = currentProcess.Handle;

            var currentProcessId = (uint) currentProcess.Id;

            MiniDumpExceptionInformation exp;

            exp.ThreadId = GetCurrentThreadId();

            exp.ClientPointers = false;

            exp.ExceptionPointers = IntPtr.Zero;

            if (exceptionInfo == ExceptionInfo.Present)

            {
                exp.ExceptionPointers = Marshal.GetExceptionPointers();
            }

            var bRet = false;

            if (exp.ExceptionPointers == IntPtr.Zero)

            {
                bRet = MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint) options, IntPtr.Zero,
                    IntPtr.Zero, IntPtr.Zero);
            }

            else

            {
                bRet = MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint) options, ref exp,
                    IntPtr.Zero, IntPtr.Zero);
            }

            return bRet;
        }


        public static bool Write(SafeHandle fileHandle, Option dumpType)

        {
            return Write(fileHandle, dumpType, ExceptionInfo.None);
        }


        //typedef struct _MINIDUMP_EXCEPTION_INFORMATION {

        //    DWORD ThreadId;

        //    PEXCEPTION_POINTERS ExceptionPointers;

        //    BOOL ClientPointers;

        //} MINIDUMP_EXCEPTION_INFORMATION, *PMINIDUMP_EXCEPTION_INFORMATION;

        [StructLayout(LayoutKind.Sequential, Pack = 4)] // Pack=4 is important! So it works also for x64!
        public struct MiniDumpExceptionInformation

        {
            public uint ThreadId;

            public IntPtr ExceptionPointers;

            [MarshalAs(UnmanagedType.Bool)] public bool ClientPointers;
        }
    }
}