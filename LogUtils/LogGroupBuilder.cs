using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Creates a new unregistered <see cref="LogGroupID"/> instance based on set build parameters.
        /// </summary>
        public LogGroupID GetID()
        {
            bool isAnonymous = string.IsNullOrWhiteSpace(Name);
            if (isAnonymous)
                return LogGroupID.Factory.CreateAnonymousGroup(Path, ModIDHint);

            return LogGroupID.Factory.CreateNamedGroup(Name, Path, ModIDHint);
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

            return LogGroupID.Factory.CreateNamedGroup(Name, Path, ModIDHint, true);
        }
    }
}
