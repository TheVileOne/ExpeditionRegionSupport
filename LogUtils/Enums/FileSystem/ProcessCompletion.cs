using System;
using System.ComponentModel;

namespace LogUtils.Enums.FileSystem
{
    public class ProcessCompletion : ExtEnum<ProcessCompletion>
    {
        /// <summary>
        /// Initializes a new <see cref="ProcessCompletion"/> instance
        /// </summary>
        /// <param name="value">The <see cref="ExtEnum{T}"/> value associated with this instance</param>
        /// <param name="register">Whether or not this instance should be registered as a unique <see cref="ExtEnum{T}"/> entry</param>
        public ProcessCompletion(string value, bool register = false) : base(value, register)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ProcessCompletion"/> instance
        /// </summary>
        /// <param name="status">The conversion value that initializes the state</param>
        /// <exception cref="InvalidEnumArgumentException">Enum value was unrecognized</exception>
        public ProcessCompletion(FileStatus status) : this(ConvertValue(status))
        {
        }

        private ProcessCompletion(ProcessCompletion status) : base(status.value, status.index != -1)
        {
        }

        static ProcessCompletion()
        {
            InitializeEnums();
        }

        internal static ProcessCompletion ConvertValue(FileStatus status)
        {
            return status switch
            {
                FileStatus.AwaitingStatus    => AwaitingStatus,
                FileStatus.NoActionRequired  => NoActionRequired,
                FileStatus.MoveRequired      => ActionRequired,
                FileStatus.MoveComplete or
                FileStatus.CopyComplete      => Complete,
                FileStatus.FileAlreadyExists => AlreadyExists,
                FileStatus.ValidationFailed  => ValidationFailed,
                FileStatus.Error             => Error,
                _ => throw new InvalidEnumArgumentException("Unexpected enum value cannot be converted. Please convert using an explicit conversion technique."),
            };
        }

        internal static void InitializeEnums()
        {
            //Order mirrors FileAction enum
            AwaitingStatus = new ProcessCompletion("AwaitingStatus", true);
            NoActionRequired = new ProcessCompletion("NoActionRequired", true);
            ActionRequired = new ProcessCompletion("ActionRequired", true);
            Complete = new ProcessCompletion("Complete", true);
            AlreadyExists = new ProcessCompletion("AlreadyExists", true);
            ValidationFailed = new ProcessCompletion("ValidationFailed", true);
            Error = new ProcessCompletion("Error", true);
        }

        /// <summary>The initial process state</summary>
        public static ProcessCompletion AwaitingStatus;
        /// <summary>Indicates that no changes were made or required. Action was already completed.</summary>
        public static ProcessCompletion NoActionRequired;
        /// <summary>Indicates that the process is incomplete</summary>
        public static ProcessCompletion ActionRequired;
        /// <summary>Indicates that requested action has been completed successfully</summary>
        public static ProcessCompletion Complete;
        /// <summary>Indicates that process did not complete due to file, or directory already existing; state is similar to <see cref="NoActionRequired"/></summary>
        public static ProcessCompletion AlreadyExists;
        /// <summary>Indicates that process did not complete due to validation process failure</summary>
        public static ProcessCompletion ValidationFailed;
        /// <summary>Indicates that process did not complete due to an <see cref="Exception"/> or other error state</summary>
        public static ProcessCompletion Error;
    }
}
