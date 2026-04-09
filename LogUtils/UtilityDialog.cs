using LogUtils.Events;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class UtilityDialog : Dialog
    {
        /// <summary>
        /// Establishes criteria for when a dialog should be scheduled
        /// </summary>
        /// <remarks>
        /// Any dialog submitted during mod initialization wont be able to activate until the end of the mod initialization frame (after post mods).<br/>
        /// This means that this will only affect the order dialogs will appear.
        /// </remarks>
        public static bool MustBeScheduled => RainWorldInfo.LatestSetupPeriodReached < SetupPeriod.PreMods;

        protected DialogState State = DialogState.NotSubmitted;

        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client
        /// </summary>
        public bool IsActive => State != DialogState.Closed && manager.dialog == this;

        /// <summary>
        /// Indicates whether the dialog is actively being displayed by the Rain World client, or is scheduled to be
        /// </summary>
        public bool IsWaitingToShow
        {
            get
            {
                return State != DialogState.Closing && State != DialogState.Closed && manager.dialog != this
                    && (UtilityCore.DialogManager.Dialogs.Contains(this) || UtilityCore.DialogManager.DialogsInQueue.Contains(this));
            }
        }

        /// <summary>
        /// A value indicating that this dialog should be closed as soon as possible
        /// </summary>
        public virtual bool WantsToClose { get; protected set; }

        /// <summary>
        /// A signal that allows closing procedure to complete
        /// </summary>
        public bool IsReadyToClose { get; protected set; } = true;

        /// <summary>
        /// Dictionary of UI descriptions
        /// </summary>
        public Dictionary<MenuObject, string> InfoText =  new Dictionary<MenuObject, string>();

        /// <summary>
        /// Event raised when this dialog enters the closing state
        /// </summary>
        public event EventHandler<UtilityDialog, EventArgs> OnClosing;

        /// <summary>
        /// Event raised when this dialog enters the closed state
        /// </summary>
        public event EventHandler<UtilityDialog, EventArgs> OnClose;

        #region Constructors
        public UtilityDialog(string description, ProcessManager manager, bool longLabel = false) : base(description, manager, longLabel)
        {
            HackHide();
        }

        public UtilityDialog(string description, ProcessManager manager) : base(description, manager)
        {
            HackHide();
        }

        public UtilityDialog(string longDescription, string title, Vector2 size, ProcessManager manager, bool longLabel = false) : base(longDescription, title, size, manager, longLabel)
        {
            HackHide();
        }

        public UtilityDialog(string description, Vector2 size, ProcessManager manager) : base(description, size, manager)
        {
            HackHide();
        }

        public UtilityDialog(ProcessManager manager) : base(manager)
        {
            HackHide();
        }
        #endregion

        public void Show()
        {
            State = DialogState.Submitted;

            UtilityLogger.Logger.LogDebug($"Dialog activated [{GetType()}]");
            UtilityLogger.Logger.LogDebug("Active process: " + manager.currentMainLoop.ID);

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
            if (manager.sideProcesses.Contains(this))
            {
                manager.StopSideProcess(this);
            }
            else if (UtilityCore.DialogManager.Dialogs.Contains(this))
            {
                manager.dialogStack.Remove(this);
            }
            else if (UtilityCore.DialogManager.DialogsInQueue.Contains(this))
            {
                for (int i = 0; i < manager._showDialogQueue.Count; i++)
                {
                    var data = manager._showDialogQueue.Dequeue();

                    if (data.Dialog == this)
                        continue;
                    manager._showDialogQueue.Enqueue(data);
                }
            }
        }

        /// <summary>
        /// Method is invoked by Rain World assembly
        /// </summary>
        public override void ShutDownProcess()
        {
            State = DialogState.Closed;

            OnClose?.Invoke(this, new EventArgs());
            base.ShutDownProcess();
        }

        /// <summary>
        /// Method is invoked by Rain World assembly
        /// </summary>
        public override void Init()
        {
            HackShow();
            infoLabel.MoveToFront();
            base.Init();
        }

        /// <summary>
        /// Method is invoked by Rain World assembly
        /// </summary>
        public override void Update()
        {
            if (!FreezeMenuFunctions) //The game doesn't set this to true until you click on the screen, which breaks hover mechanics
                manager.menuesMouseMode = true;

            if (State == DialogState.Closed)
            {
                base.Update();
                return;
            }

            if (WantsToClose && State != DialogState.Closing)
            {
                State = DialogState.Closing;
                OnClosing?.Invoke(this, EventArgs.Empty);
            }

            base.Update();

            if (IsReadyToClose && State == DialogState.Closing)
                Dismiss();
        }

        /// <summary>
        /// Method is invoked by Rain World assembly
        /// </summary>
        public override string UpdateInfoText()
        {
            string infoText = base.UpdateInfoText();

            if (selectedObject == null || !string.IsNullOrEmpty(infoText))
                return infoText;

            if (InfoText.TryGetValue(selectedObject, out string value))
                infoText = Translate(value);
            return infoText;
        }
    }

    public enum DialogState
    {
        NotSubmitted = -1, //Instance has yet to be submitted to Rain World's dialog stack
        Submitted = 0,     //Instance is currently submitted to Rain World's dialog stack
        Closing = 1,       //Instance is slated to be closed
        Closed = 2,        //Instance is in the process of being unloaded
    }
}
