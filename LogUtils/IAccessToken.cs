using System;

namespace LogUtils
{
    /// <summary>
    /// Interface for facilitating access to a temporary folder
    /// </summary>
    public interface IAccessToken : IDisposable
    {
        /// <summary>
        /// Signal that a process intends to access and use a temporary folder. While accessing, LogUtils guarantees that the folder wont be moved, or deleted
        /// through the <see cref="TempFolderInfo"/> public API. Call <see cref="RevokeAccess"/> to signal that your process no longer needs to access the temporary folder.
        /// </summary>
        /// <remarks>For each time this method is called, a following <see cref="RevokeAccess"/> must also be called.</remarks>
        IAccessToken Access();

        /// <summary>
        /// Signal that a process no longer needs to access any data located inside of a temporary folder. Do not call this unless your process
        /// already has access. Doing so may corrupt/remove data being used by other processes that require this temporary folder.
        /// </summary>
        void RevokeAccess();
    }
}
