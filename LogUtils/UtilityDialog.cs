using Menu;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class UtilityDialog : Dialog
    {
        private State state = State.NotSubmitted;

        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client
        /// </summary>
        public bool IsActive => state != State.Closed && manager.dialog == this;

        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client, or is scheduled to be
        /// </summary>
        public bool IsPending => state != State.Closed && manager.dialog != this && UtilityCore.CurrentDialogs.Contains(this);

        /// <summary>
        /// Event raised when this dialog is closing
        /// </summary>
        public event Events.EventHandler<UtilityDialog, DialogCloseEventArgs> OnClose;

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
            state = State.Submitted;

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
            state = State.Closed;

            OnClose?.Invoke(this, new DialogCloseEventArgs());
            base.ShutDownProcess();
        }

        private enum State
        {
            NotSubmitted = -1, //Instance has yet to be submitted to Rain World's dialog stack
            Submitted = 0,     //Instance is currently submitted to Rain World's dialog stack
            Closed = 1,        //Instanceis in the process of being unloaded
        }
    }

    public class DialogCloseEventArgs : EventArgs;
}
