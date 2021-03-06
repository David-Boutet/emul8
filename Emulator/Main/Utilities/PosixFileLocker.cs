#if !EMUL8_PLATFORM_WINDOWS
//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace Emul8.Utilities
{
    public class PosixFileLocker : IDisposable
    {
        public PosixFileLocker(string fileToLock)
        {
            file = fileToLock;
            fd = Syscall.open(fileToLock, OpenFlags.O_CREAT | OpenFlags.O_RDWR, FilePermissions.DEFFILEMODE);
            if(!TryDoFileLocking(fd, true))
            {
                throw new InvalidOperationException("File {0} not locked.".FormatWith(file));
            }
        }

        public void Dispose()
        {
            if(!TryDoFileLocking(fd, false))
            {
                throw new InvalidOperationException("File {0} not unlocked.".FormatWith(file));
            }
            File.Delete(file);
        }

        private static bool TryDoFileLocking(int fd, bool lockFile, FlockOperation? specificFlag = null)
        {
            if (fd >= 0) 
            {
                int res;
                Errno lastError;
                do
                {
                    res = Flock(fd, specificFlag ?? (lockFile ? FlockOperation.LOCK_EX : FlockOperation.LOCK_UN));
                    lastError = Stdlib.GetLastError();
                }
                while(res != 0 && lastError == Errno.EINTR);
                // if can't get lock ...
                return res == 0;
            } 
            return false;
        }

        private readonly int fd;
        private readonly string file;

        [DllImport("libc", EntryPoint = "flock")]
        private extern static int Flock(int fd, FlockOperation operation);

        [Flags]
        private enum FlockOperation
        {
            LOCK_SH = 1,
            LOCK_EX = 2,
            LOCK_NB = 4,
            LOCK_UN = 8
        }
    }
}
#endif
