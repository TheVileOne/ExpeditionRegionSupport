using LogUtils.Events;
using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static LogUtils.FolderActivityManager;
using static LogUtils.UtilityConsts;

namespace LogUtils
{
    public class ConflictResolutionDialog : UtilityDialog
    {
        internal const string DESCRIPTION = "Attempt to move files unable to continue:   {0}";

        internal const float PADDING_X = 90f;
        internal const float PADDING_Y = 15f;

        internal const float LABEL_HEIGHT = 24f;

        private Vector2 detailsPosition;
        private PositionedMenuObject detailsContainer;

        private readonly MergeHistory history;
        private readonly ConflictResolutionHandler handler;
        private MergeRecord activeConflict;

        private int conflictsRemaining
        {
            get
            {
                int activeCount = history.Conflicts.Count;
                int skippedCount = handler.Feedback.Count(f => f == ConflictResolutionFeedback.SaveForLater);

                int result = 0;
                if (activeConflict == null)
                {
                    result = activeCount + skippedCount;
                }
                else if (activeCount > 0)
                {
                    result = activeCount + skippedCount + 1;
                }
                else if (skippedCount > 0)
                {
                    result = skippedCount + (activeConflict != skippedEntries.Current ? 1 : 0); //Avoids double counting
                }
                else
                {
                    result = 1; //Only conflict is the active conflict
                }
                return result;
            }
        }

        private IEnumerator<MergeRecord> skippedEntries;

        public ConflictResolutionDialog(MergeHistory history) : base(createDescription(history.Conflicts.Count, history.Conflicts.Peek()), calculateSize(history), RainWorldInfo.RainWorld.processManager)
        {
            this.handler = new ConflictResolutionHandler();
            this.history = history;

            //Old value: 97.5f
            const float DESCRIPTION_LABEL_OFFSET = 157.5f; //Defines the spacing between the top of the screen.
                                                           //A higher value means label draws higher on the screen.
            const float DETAILS_LABEL_OFFSET = 115f;       //Defines the spacing between the description label and the detail labels.
                                                           //A higher value means places detail labels closer to description label.

            descriptionLabel.pos.y = pos.y + DESCRIPTION_LABEL_OFFSET;

            //UtilityLogger.Log("Rect position " + roundedRect.pos);
            //UtilityLogger.Log("Rect size " + roundedRect.size);
            //UtilityLogger.Log("Label position " + descriptionLabel.pos);

            //Set initial position to top-left position from bottom-left aligned position
            Vector2 initialPosition = new Vector2(roundedRect.pos.x, roundedRect.pos.y + roundedRect.size.y);
            Vector2 currentPosition = new Vector2(initialPosition.x, descriptionLabel.pos.y + DETAILS_LABEL_OFFSET);

            detailsPosition = currentPosition;
            updateDetails();
            currentPosition.y -= LABEL_HEIGHT * detailsContainer.subObjects.Count + 1; //Account for label positions

            Vector2 labelPosition = new Vector2(currentPosition.x + PADDING_X, currentPosition.y - 24f);
            MenuLabel label = new MenuLabel(this, dialogPage, "Select option to continue", labelPosition, Vector2.zero, false);

            label.label.alignment = FLabelAlignment.Left;
            dialogPage.subObjects.Add(label);

            currentPosition.y -= PADDING_Y * 6; //Set some extra space between the options and the info section

            Vector2 buttonPosition;
            Vector2 buttonSize = new Vector2(150f, 30f);
            float buttonSpacing = PADDING_X;

            //TODO: Figure out why this assignment does not work
            float expectedTotalButtonsWidth = (buttonSize.x * 4) + (buttonSpacing * 3);
            float buttonPadding = 160f; //(roundedRect.size.x - expectedTotalButtonsWidth) / 2;

            buttonPosition = new Vector2(buttonPadding + currentPosition.x + (buttonSize.x / 2), currentPosition.y);
            dialogPage.subObjects.Add(new SimpleButton(this, dialogPage, Translate("Overwrite"), DialogOption.OVERWRITE, buttonPosition, buttonSize));

            buttonPosition = new Vector2(buttonPosition.x + (buttonSize.x + buttonSpacing) - (buttonSize.x / 2), currentPosition.y);
            dialogPage.subObjects.Add(new SimpleButton(this, dialogPage, Translate("Keep Both"), DialogOption.KEEP_BOTH, buttonPosition, buttonSize));

            buttonPosition = new Vector2(buttonPosition.x + (buttonSize.x + buttonSpacing) - (buttonSize.x / 2), currentPosition.y);
            dialogPage.subObjects.Add(new SimpleButton(this, dialogPage, Translate("Skip For Now"), DialogOption.SKIP, buttonPosition, buttonSize));

            buttonPosition = new Vector2(buttonPosition.x + (buttonSize.x + buttonSpacing) - (buttonSize.x / 2), currentPosition.y);
            dialogPage.subObjects.Add(new SimpleButton(this, dialogPage, Translate("Cancel"), DialogOption.CANCEL, buttonPosition, buttonSize));

            OnClose += conflictResolutionDialog_OnClose;
        }

        private void conflictResolutionDialog_OnClose(UtilityDialog source, EventArgs data)
        {
            //Ensure that files are always cleaned up even on a sudden closure of the dialog
            if (activeConflict != null)
                Singal(null, DialogOption.CANCEL);
        }

        /// <summary>
        /// Indicates that there are pending, or active dialogs for this type
        /// </summary>
        public static bool HasAnyDialogs => UtilityCore.DialogManager.Dialogs.Where(dialog => dialog.IsActive || dialog.IsWaitingToShow).ContainsType<ConflictResolutionDialog>()
                                         || UtilityCore.DialogManager.DialogsInQueue.ContainsType<ConflictResolutionDialog>();

        public static void ShowDialog(MergeHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            ActivityRecord record = LogFolderInfo.ActivityManager.GetRecord(history);

            if (record == null || record.State > ActivityState.WaitingForConflictResolution)
            {
                UtilityLogger.Log("Conflict resolution is no longer required");
                return;
            }

            //It is important that folder permissions be assigned before the user interacts with this dialog. Dialog can only run after mod init completes
            //and game will continue to process update frames while dialog is running giving a window for enabled mods to assign folder permissions.
            if (!MustBeScheduled)
            {
                UtilityDialog dialog = new ConflictResolutionDialog(history);
                dialog.Show();
                return;
            }

            UtilityLogger.Log("Scheduling merge conflict resolution");
            UtilityEvents.OnSetupPeriodReached += scheduledEvent;

            void scheduledEvent(SetupPeriodEventArgs e)
            {
                if (e.CurrentPeriod < SetupPeriod.PreMods)
                    return;

                if (record.State > ActivityState.WaitingForConflictResolution)
                {
                    UtilityLogger.Log("Conflict resolution is no longer required");
                    UtilityEvents.OnSetupPeriodReached -= scheduledEvent;
                    return;
                }

                UtilityDialog dialog = new ConflictResolutionDialog(history);
                dialog.Show();
                UtilityEvents.OnSetupPeriodReached -= scheduledEvent;
            }
        }

        private static string createDescription(int conflictCount, MergeRecord activeRecord)
        {
            switch (conflictCount)
            {
                case 0:
                    return "ERROR: No conflicts detected. Please report this.";
                case 1:
                    return string.Format(DESCRIPTION, $"Destination already has a file named `{Path.GetFileName(activeRecord.CurrentPath)}`.");
                default:
                    return string.Format(DESCRIPTION, $"Destination has {conflictCount} files with the same name.");
            }
        }

        private static Vector2 calculateSize(MergeHistory history)
        {
            const int DETAIL_WIDTH_BASE = 90; //The current size of unchanging label text
            const int MIN_WIDTH = 800;        //The minimum width of the dialog at any label size

            float height, width;

            height = 400.01f; //0.01 is important here (LABEL FIX)

            float longestDetail = 0f;
            foreach (MergeRecord record in history.Conflicts)
            {
                float detailWidth = Mathf.Max(LabelTest.GetWidth(record.CurrentPath), LabelTest.GetWidth(record.OriginalPath))
                                  + DETAIL_WIDTH_BASE + (PADDING_X * 2); //Account for margins
                if (detailWidth > longestDetail)
                    longestDetail = detailWidth;
            }

            UtilityLogger.Logger.LogDebug($"Longest label is {longestDetail} units");
            width = Mathf.Max(longestDetail, MIN_WIDTH) + 0.01f; //Clamp to the minimum allowed value
            return new Vector2(width, height);
        }

        /// <summary>
        /// Collects feedback results based on user-selected options
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="message">Case-sensitive identifier</param>
        public override void Singal(MenuObject sender, string message)
        {
            if (activeConflict == null && message != DialogOption.CANCEL)
            {
                base.Singal(sender, message);
                return;
            }

            UtilityLogger.Log("OPTION SELECTED: " + message);
            switch (message)
            {
                case DialogOption.CANCEL:
                    history.Restore();
                    lock (LogFolderInfo.ActivityManager)
                    {
                        ActivityRecord record = LogFolderInfo.ActivityManager.GetRecord(history);

                        if (record != null)
                        {
                            record.State = ActivityState.Faulted;
                            record.Events.RaiseEvent(MergeEventID.Canceled);
                            LogFolderInfo.ActivityManager.RemoveRecord(record);
                        }
                    }
                    activeConflict = null;
                    Dismiss();
                    break;
                case DialogOption.LEAVE_FILE:
                    //TODO: File-specific cancellation
                    handler.CollectFeedback(ConflictResolutionFeedback.CancelMove, activeConflict);
                    break;
                case DialogOption.OVERWRITE:
                    handler.CollectFeedback(ConflictResolutionFeedback.Overwrite, activeConflict);
                    break;
                case DialogOption.KEEP_BOTH:
                    handler.CollectFeedback(ConflictResolutionFeedback.KeepBoth, activeConflict);
                    break;
                case DialogOption.SKIP:
                    handler.CollectFeedback(ConflictResolutionFeedback.SaveForLater, activeConflict);
                    break;
                default:
                    base.Singal(sender, message);
                    return;
            }
            activeConflict = null;
        }

        /// <summary/>
        public override void Update()
        {
            base.Update();

            if (State == DialogState.Closed) return;

            if (activeConflict == null)
            {
                updateActiveConflict();
                if (activeConflict == null)
                {
                    try
                    {
                        handler.ResolveAll();
                        lock (LogFolderInfo.ActivityManager)
                        {
                            ActivityRecord record = LogFolderInfo.ActivityManager.GetRecord(history);

                            if (record != null)
                            {
                                record.State = ActivityState.Completed;
                                record.Events.RaiseEvent(MergeEventID.Completed);
                                LogFolderInfo.ActivityManager.RemoveRecord(record);
                            }
                        }
                        UtilityLogger.Log("No more conflicts");
                    }
                    catch (OperationCanceledException ex) //User chose to cancel merge, or there was a failure to resolve
                    {
                        history.HasFailed = true;
                        history.Exception = ex;
                    }
                    finally
                    {
                        while (handler.ResolvedEntries.Count > 0)
                        {
                            MergeRecord current = handler.ResolvedEntries.Dequeue();

                            if (!current.IsCanceled)
                                history.AddRecord(current);
                        }
                    }

                    if (!history.HasFailed)
                    {
                        Dismiss();
                    }
                    else
                    {
                        UtilityLogger.LogError("Unable to complete merge", history.Exception);
                        Singal(null, DialogOption.CANCEL);
                    }
                    return;
                }
                updateDetails();
            }
        }

        private void updateActiveConflict()
        {
            if (history.Conflicts.Count > 0)
            {
                activeConflict = history.Conflicts.Dequeue();
                return;
            }

            if (skippedEntries == null)
                skippedEntries = handler.GetSkippedConflicts();

            if (!skippedEntries.MoveNext())
            {
                //User may have skipped more entries
                skippedEntries.Dispose();
                skippedEntries = handler.GetSkippedConflicts();

                if (skippedEntries.MoveNext())
                    activeConflict = skippedEntries.Current;
            }
            else
            {
                activeConflict = skippedEntries.Current;
            }
        }

        private void updateDetails()
        {
            string[] detailLabels =
            {
                //Current path
                "CURRENT:         \'{0}\'", //Spaces align with destination label
                //Destination path
                "DESTINATION: \'{0}\'",
            };

            if (detailsContainer == null)
            {
                detailsContainer = new MenuContainer(this, dialogPage, detailsPosition);
                dialogPage.subObjects.Add(detailsContainer);

                for (int i = 0; i < detailLabels.Length; i++)
                {
                    float labelOffsetY = i > 0 ? LABEL_HEIGHT * i : 0;

                    Vector2 labelPosition = new Vector2(detailsPosition.x + PADDING_X, detailsPosition.y - labelOffsetY);
                    MenuLabel label = new MenuLabel(this, dialogPage, string.Format(detailLabels[i], "N/A"), labelPosition, Vector2.zero, false);

                    label.label.alignment = FLabelAlignment.Left;
                    detailsContainer.subObjects.Add(label);
                }
            }
            else
            {
                descriptionLabel.text = createDescription(conflictsRemaining, activeConflict);

                MenuLabel currentLabel = (MenuLabel)detailsContainer.subObjects[0];
                MenuLabel destinationLabel = (MenuLabel)detailsContainer.subObjects[1];

                currentLabel.text = string.Format(detailLabels[0], activeConflict.OriginalPath);
                destinationLabel.text = string.Format(detailLabels[1], activeConflict.CurrentPath);
            }
        }
    }
}
