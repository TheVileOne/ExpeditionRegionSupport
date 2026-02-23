using System;

namespace LogUtils.Enums.FileSystem
{
    /// <summary>
    /// Represents permission flags that apply to a folder
    /// </summary>
    [Flags]
    public enum FolderPermissions
    {
        /// <summary>The default state</summary>
        Invalid = -1,
        /// <summary>Permission state that does not permit most folder operations</summary>
        None = 0,
        /// <summary>Permission state that gives LogUtils deletion privileges</summary>
        Delete = 1,
        /// <summary>Permission state that gives LogUtils move privileges</summary>
        Move = 2,
        /// <summary>Permission state that gives all privileges</summary>
        All = Delete | Move
    }
}
