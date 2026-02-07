using System;
using System.ComponentModel;

namespace LogUtils.Enums.FileSystem
{
    public class ActionStatus : ExtEnum<ActionStatus>
    {
        /// <summary>
        /// Initializes a new <see cref="ActionStatus"/> instance
        /// </summary>
        /// <param name="value">The <see cref="ExtEnum{T}"/> value associated with this instance</param>
        /// <param name="register">Whether or not this instance should be registered as a unique <see cref="ExtEnum{T}"/> entry</param>
        public ActionStatus(string value, bool register = false) : base(value, register)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ActionStatus"/> instance
        /// </summary>
        /// <param name="status">The conversion value that initializes the state</param>
        /// <exception cref="InvalidEnumArgumentException">Enum value was unrecognized</exception>
        public ActionStatus(FileStatus status) : this(ConvertValue(status))
        {
        }

        private ActionStatus(ActionStatus status) : base(status.value, status.index != -1)
        {
        }

        static ActionStatus()
        {
            InitializeEnums();
        }

        internal static ActionStatus ConvertValue(FileStatus status)
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
            None = new ActionStatus("None", true);
            AwaitingStatus = new ActionStatus("AwaitingStatus", true);
            NoActionRequired = new ActionStatus("NoActionRequired", true);
            ActionRequired = new ActionStatus("ActionRequired", true);
            Aborted = new ActionStatus("Aborted", true);
            Complete = new ActionStatus("Complete", true);
            AlreadyExists = new ActionStatus("AlreadyExists", true);
            ValidationFailed = new ActionStatus("ValidationFailed", true);
            Error = new ActionStatus("Error", true);

            Pending = AwaitingStatus; //Alias
        }

        /// <summary>No status</summary>
        public static ActionStatus None;
        /// <summary>The initial process state</summary>
        public static ActionStatus AwaitingStatus, Pending;
        /// <summary>Indicates that no changes were made or required. Action was already completed.</summary>
        public static ActionStatus NoActionRequired;
        /// <summary>Indicates that the process is incomplete</summary>
        public static ActionStatus ActionRequired;
        /// <summary>Indicates that the process has been cancelled</summary>
        public static ActionStatus Aborted;
        /// <summary>Indicates that requested action has been completed successfully</summary>
        public static ActionStatus Complete;
        /// <summary>Indicates that process did not complete due to file, or directory already existing; state is similar to <see cref="NoActionRequired"/></summary>
        public static ActionStatus AlreadyExists;
        /// <summary>Indicates that process did not complete due to validation process failure</summary>
        public static ActionStatus ValidationFailed;
        /// <summary>Indicates that process did not complete due to an <see cref="Exception"/> or other error state</summary>
        public static ActionStatus Error;
    }
}
