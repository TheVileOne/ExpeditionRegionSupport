using System;

namespace LogUtils.Properties
{
    /// <summary>
    /// Interface for accessing a <see cref="LogProperties"/> instance
    /// </summary>
    public interface IPropertyHolder : IEquatable<LogProperties>
    {
        /// <summary>
        /// Contains metadata information (filename, path, etc), and other settings that impact logging behavior, and log file management
        /// </summary>
        LogProperties Properties { get; }
    }
}
