using System;

namespace LogUtils.Enums.FileSystem
{
    /// <summary>
    /// Represents permission flags that apply to a folder
    /// </summary>
    [Flags]
    public enum FolderPermissions
    {
        None = 0,
        Delete = 1,
        Move = 2,
        All = Delete | Move
    }
}
