using System.ComponentModel;

namespace LogUtils.Enums.FileSystem
{
    //TODO: Implement IExtEnumBase
    public class ActionType : ExtEnum<ActionType>
    {
        /// <summary>
        /// Initializes a new <see cref="ActionType"/> instance
        /// </summary>
        /// <param name="value">The <see cref="ExtEnum{T}"/> value associated with this instance</param>
        /// <param name="register">Whether or not this instance should be registered as a unique <see cref="ExtEnum{T}"/> entry</param>
        public ActionType(string value, bool register = false) : base(value, register)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ActionType"/> instance
        /// </summary>
        /// <param name="action">The conversion value that initializes the state</param>
        /// <exception cref="InvalidEnumArgumentException">Enum value was unrecognized</exception>
        public ActionType(FileAction action) : this(ConvertValue(action))
        {
        }

        private ActionType(ActionType action) : base(action.value, action.index != -1)
        {
        }

        static ActionType()
        {
            InitializeEnums();
        }

        internal static ActionType ConvertValue(FileAction action)
        {
            return action switch
            {
                FileAction.None => None,
                FileAction.Create => Create,
                FileAction.Delete => Delete,
                FileAction.Buffering => Buffering,
                FileAction.Write => Write,
                FileAction.Move => Move,
                FileAction.Copy => Copy,
                FileAction.Open => Open,
                FileAction.PathUpdate => PathUpdate,
                FileAction.SessionStart => SessionStart,
                FileAction.SessionEnd => SessionEnd,
                FileAction.StreamDisposal => StreamDisposal,
                _ => throw new InvalidEnumArgumentException("Unexpected enum value cannot be converted. Please convert using an explicit conversion technique."),
            };
        }

        internal static void InitializeEnums()
        {
            //Order mirrors FileAction enum
            None = new ActionType("None", true);
            Create = new ActionType("Create", true);
            Delete = new ActionType("Delete", true);
            Buffering = new ActionType("Buffering", true);
            Write = new ActionType("Write", true);
            Move = new ActionType("Move", true);
            Copy = new ActionType("Copy", true);
            Open = new ActionType("Open", true);
            PathUpdate = new ActionType("PathUpdate", true);
            SessionStart = new ActionType("SessionStart", true);
            SessionEnd = new ActionType("SessionEnd", true);
            StreamDisposal = new ActionType("StreamDisposal", true);
        }

        /// <summary>Default value</summary>
        public static ActionType None;
        /// <summary>Represents a file, or directory creation event</summary>
        public static ActionType Create;
        /// <summary>Represents a file, or directory deletion event</summary>
        public static ActionType Delete;
        /// <summary>Represents a file stream buffer event</summary>
        public static ActionType Buffering;
        /// <summary>Represents a file write event</summary>
        public static ActionType Write;
        /// <summary>Represents a file, or directory move or rename event</summary>
        public static ActionType Move;
        /// <summary>Represents a file, or directory copy event</summary>
        public static ActionType Copy;
        /// <summary>Represents a file (or filestream) open event</summary>
        public static ActionType Open;
        /// <summary>Represents a path update event</summary>
        public static ActionType PathUpdate;
        /// <summary>Represents a log session started event</summary>
        public static ActionType SessionStart;
        /// <summary>Represents a log session ended event</summary>
        public static ActionType SessionEnd;
        /// <summary>Represents a file stream disposal event</summary>
        public static ActionType StreamDisposal;
    }
}
