using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Threading;

namespace LogUtils.Enums
{
    /// <summary>
    /// A type of <see cref="LogID"/> representing a log file group.
    /// </summary>
    /// <remarks>
    /// Log group properties may be accessed, and changed through the <see cref="Properties"/> field.
    /// </remarks>
    public partial class LogGroupID : LogID
    {
        /// <summary>
        /// This prefix differentiates log group entries from log file entries
        /// </summary>
        internal const string ID_PREFIX = $"{UtilityConsts.PropertyTag.LOG_GROUP}:";

        private static int _nextID = -1;

        /// <summary>
        /// Registration may be handled through the <see cref="SharedExtEnum{T}"/> constructor only when no other existing reference to this <see cref="LogID"/> value is present.  
        /// </summary>
        protected override RegistrationStatus RegistrationStage
        {
            get
            {
                RegistrationStatus stage = base.RegistrationStage;

                //Inherit the status when registration is already completed
                if (stage == RegistrationStatus.Completed)
                    return RegistrationStatus.Completed;

                if (Properties == null) //Log groups needs to check the property state for validation purposes
                    return RegistrationStatus.WaitingOnSignal;
                return stage;
            }
        }

        /// <inheritdoc/>
        public override string Tag
        {
            get
            {
                if (RegistrationStage == RegistrationStatus.Completed && !ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Tag;

                return Value; //Unlike typical LogID types, groups do not use the path as an identifier. There can be multiple groups with the same specified path.
            }
        }

        /// <inheritdoc cref="IPropertyHolder.Properties"/>
        public new LogGroupProperties Properties
        {
            get => (LogGroupProperties)base.Properties;
            protected set => base.Properties = value;
        }

        /// <inheritdoc cref="LogGroupID(string, string, bool)"/>
        public LogGroupID(string value, bool register = false) : base(getProperties(value), register)
        {
            Properties.InitializePermissions();
        }

        /// <summary>
        /// Creates a new <see cref="LogGroupID"/> instance.
        /// </summary>
        /// <inheritdoc cref="LogID(string, LogAccess, bool)"/>
        /// <param name="value">The value that identifies the <see cref="LogGroupID"/> instance</param>
        /// <param name="path">An optional path that all group members will have in common</param>
        /// <param name="register"></param>
        public LogGroupID(string value, string path, bool register = false) : base(getProperties(value), register)
        {
            Properties.InitializePermissions();
            Properties.SetInitialPath(path);
        }

        internal LogGroupID(LogProperties properties, bool register) : base(properties, register)
        {
        }

        internal void Assign(LogID target)
        {
            target.Properties.Group = this;
            Properties.Members.Add(target);
            Properties.ReadOnly = true;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Log group is anonymous</exception>
        public override void Register()
        {
            if (Properties.IsAnonymous)
                throw new InvalidOperationException("Anonymous groups cannot be registered");

            base.Register();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.Substring(ID_PREFIX.Length);
        }

        /// <inheritdoc/>
        public override bool CheckTag(string tag)
        {
            //Adding a file extension is required by the helper
            if (PathUtils.IsFilePath(tag + ".txt")) //LogGroupIDs do not store path information in the tags - this should never be a match
                return false;

            return ComparerUtils.StringComparerIgnoreCase.Equals(Tag, tag);
        }

        internal static string CreateIDValue(string value, out bool isAnonymous)
        {
            isAnonymous = string.IsNullOrWhiteSpace(value);
            if (isAnonymous)
            {
                int uniqueGroupID = Interlocked.Increment(ref _nextID);
                value = "<>g__Anonymous" + uniqueGroupID;
            }
            return CreateIDValue(value, LogIDType.Group);
        }

        private static LogProperties getProperties(string value)
        {
            value = CreateIDValue(value, out bool isAnonymous); //Expecting an unformatted value here

            if (!isAnonymous) //Anonymous groups cannot be matched
            {
                //Inherit properties from an existing group ID if one exists, or create new properties
                LogID found = Find(value, CompareOptions.ID, includeGroupIDs: true);

                if (found != null)
                    return found.Properties;
            }
            return new LogGroupProperties(value) { IsAnonymous = isAnonymous };
        }
    }
}
