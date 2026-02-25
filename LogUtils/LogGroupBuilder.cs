using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    public class LogGroupBuilder
    {
        /// <summary>
        /// Unique identifying value for the group.
        /// </summary>
        /// <value>If this field is not set, the log group may not by registered.</value>
        public string Name;

        /// <summary>
        /// Default folder location (including folder name) of log group files.
        /// </summary>
        /// <value>This should be a fully qualified, relative, or partial path where group files will be stored. Leave empty to not associate with any path.</value>
        public string Path;

        /// <summary>
        /// The plugin ID that identifies a mod specific folder location to associate with the group path. 
        /// </summary>
        /// <value>This should be set if you want to log to a location inside the Mods directory. Leave empty for any other location.</value>
        public string ModIDHint;

        /// <summary>
        /// Flag indicates that the builder has permissions to create a log group folder and any subfolders
        /// </summary>
        public bool CanCreateFolder { get; protected set; }

        /// <summary>
        /// Collection of subfolder names, or partial paths containing subfolders names to create on building a <see cref="LogGroupID"/> through use of the builder
        /// </summary>
        public ICollection<string> SubFolders { get; protected set; } = [];

        /// <summary>
        /// Attempt to create the group folder, and any specified subpaths on the creation of the group ID (only applies to folder groups)
        /// </summary>
        public void CreateFolderOnBuild()
        {
            CanCreateFolder = true;
        }

        /// <summary>
        /// Creates a new unregistered <see cref="LogGroupID"/> instance based on set build parameters.
        /// </summary>
        public LogGroupID GetID()
        {
            LogGroupID result;
            bool isAnonymous = string.IsNullOrWhiteSpace(Name);
            if (isAnonymous)
                result = LogGroupID.Factory.CreateAnonymousGroup(Path, ModIDHint);

            result = LogGroupID.Factory.CreateNamedGroup(Name, Path, ModIDHint);

            if (CanCreateFolder && result.Properties.IsFolderGroup)
                CreateGroupFolder(result);
            return result;
        }

        /// <summary>
        /// Creates a new registered <see cref="LogGroupID"/> instance based on set build parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">"Name field was not set."</exception>
        public LogGroupID GetRegisteredID()
        {
            bool isAnonymous = string.IsNullOrWhiteSpace(Name);
            if (isAnonymous)
                throw new InvalidOperationException("Anonymous groups cannot be registered");

            LogGroupID result = LogGroupID.Factory.CreateNamedGroup(Name, Path, ModIDHint, true);

            if (CanCreateFolder && result.Properties.IsFolderGroup)
                CreateGroupFolder(result);
            return result;
        }

        protected void CreateGroupFolder(LogGroupID group)
        {
            //Create group folder
            string groupPath = group.Properties.CurrentFolderPath;
            Directory.CreateDirectory(groupPath);

            //Create subfolders inside the group folder
            foreach (string folder in SubFolders)
                Directory.CreateDirectory(combinePath(groupPath, folder));

            static string combinePath(string basePath, string subPath)
            {
                return System.IO.Path.Combine(basePath, subPath.TrimStart(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
            }
        }
    }
}
