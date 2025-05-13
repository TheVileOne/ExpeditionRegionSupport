using LogUtils.Diagnostics;
using System;

namespace LogUtils.IPC
{
    public static class ErrorCodeProvider
    {
        public static ErrorCode GetCode(Exception exception)
        {
            Condition.Assert(exception is not AggregateException);

            string message = exception.Message;

            if (message.StartsWith("There is a process"))
                return ErrorCode.ProcessAlreadyExists;

            if (message.StartsWith("All pipe instances are busy"))
                return ErrorCode.PipeBusy;

            if (message.StartsWith("Pipe is not connected") || message.StartsWith("Win32 IO returned 233"))
                return ErrorCode.PipeDisconnected;

            return ErrorCode.Unknown;
        }
    }

    public enum ErrorCode
    {
        PipeBusy,
        PipeDisconnected,
        PipeAlreadyExists,
        ProcessAlreadyExists,
        Unknown
    }
}
