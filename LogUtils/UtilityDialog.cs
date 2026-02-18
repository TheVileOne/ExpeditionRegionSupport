using Menu;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class UtilityDialog : Dialog
    {
        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client
        /// </summary>
        public bool IsActive => manager.dialog == this;

        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client, or is scheduled to be
        /// </summary>
        public bool IsActiveOrPending => UtilityCore.CurrentDialogs.Contains(this);

        public event Events.EventHandler<UtilityDialog, DialogCloseEventArgs> OnClosing;

        #region Constructors
        public UtilityDialog(string description, ProcessManager manager, bool longLabel = false) : base(description, manager, longLabel)
        {
        }

        public UtilityDialog(string description, ProcessManager manager) : base(description, manager)
        {
        }

        public UtilityDialog(string longDescription, string title, Vector2 size, ProcessManager manager, bool longLabel = false) : base(longDescription, title, size, manager, longLabel)
        {
        }

        public UtilityDialog(string description, Vector2 size, ProcessManager manager) : base(description, size, manager)
        {
        }

        public UtilityDialog(ProcessManager manager) : base(manager)
        {
        }
        #endregion

        public void Show()
        {
            UtilityLogger.Log("Dialog activated");
            UtilityLogger.Log("Active process: " + manager.currentMainLoop.ID);

            if (!manager.currentMainLoop.AllowDialogs) //Most menus will support dialogs
                UtilityLogger.LogWarning("Active process does not allow dialogs");

            if (!ProcessManager.fontHasBeenLoaded || manager.IsSwitchingProcesses())
                UtilityLogger.LogWarning("Dialog has been added to the show queue");

            manager.ShowDialog(this);
        }

        /// <summary>
        /// Stops the dialog process
        /// </summary>
        /// <remarks>This will always invoke close event even if dialog is not active</remarks>
        public void Dismiss()
        {
            manager.StopSideProcess(this);
        }

        /// <summary>
        /// Method is invoked by Rain World assembly
        /// </summary>
        public override void ShutDownProcess()
        {
            OnClosing?.Invoke(this, new DialogCloseEventArgs());
            base.ShutDownProcess();
        }
    }

    public class DialogCloseEventArgs : EventArgs;
}
