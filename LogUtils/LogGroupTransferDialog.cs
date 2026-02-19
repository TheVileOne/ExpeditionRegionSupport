using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Linq;
using UnityEngine;
using DialogOption = LogUtils.UtilityConsts.DialogOption;

namespace LogUtils
{
    public class LogGroupTransferDialog : UtilityDialog
    {
        internal const string DESCRIPTION = "A log group change has been detected that requires your input.";

        internal const float PADDING_X = 90f;
        internal const float PADDING_Y = 15f;

        internal const float LABEL_HEIGHT = 24f;

        private readonly LogGroupID groupID;
        private readonly string destinationPath;

        private RadioButtonGroup dialogOptions;

        public LogGroupTransferDialog(LogGroupID groupID, string destinationPath) : base(DESCRIPTION, calculateSize(groupID, destinationPath), RainWorldInfo.RainWorld.processManager)
        {
            this.groupID = groupID;
            this.destinationPath = destinationPath;

            descriptionLabel.pos.y = pos.y + 185f;

            //UtilityLogger.Log("Rect position " + roundedRect.pos);
            //UtilityLogger.Log("Rect size " + roundedRect.size);
            //UtilityLogger.Log("Label position " + descriptionLabel.pos);

            //Set initial position to top-left position from bottom-left aligned position
            Vector2 initialPosition = new Vector2(roundedRect.pos.x, roundedRect.pos.y + roundedRect.size.y);

            Vector2 currentPosition = new Vector2(initialPosition.x, initialPosition.y - 120f); //Align to a position after the description label. Size of description label was not useful.
            addDetails(ref currentPosition);

            currentPosition.y -= PADDING_Y * 3; //Set some extra space between the options and the info section

            MenuLabel label = new MenuLabel(this, dialogPage, "Resolution Options", new Vector2(currentPosition.x + PADDING_X, currentPosition.y), Vector2.zero, false);

            label.label.alignment = FLabelAlignment.Left;
            dialogPage.subObjects.Add(label);

            currentPosition.y -= LABEL_HEIGHT + PADDING_Y;
            addOptions(ref currentPosition);

            currentPosition.y -= PADDING_Y * 2;

            Vector2 buttonSize = new Vector2(210.5f, 30f);
            Vector2 buttonPosition = new Vector2(currentPosition.x + (size.x / 2) - (buttonSize.x / 2), currentPosition.y);
            SimpleButton acceptButton = new SimpleButton(this, dialogPage, Translate("Accept"), DialogOption.ACCEPT, buttonPosition, buttonSize);
            dialogPage.subObjects.Add(acceptButton);
        }

        /// <summary>
        /// Indicates that there are pending, or active dialogs for this type
        /// </summary>
        public static bool HasAnyDialogs => UtilityCore.CurrentDialogs.Where(dialog => dialog.IsActive || dialog.IsPending).ContainsType<LogGroupTransferDialog>();

        /// <summary>
        /// Shows a dialog presenting options on how to transfer a log group to a specified path
        /// </summary>
        public static void ShowDialog(LogGroupID groupID, string pendingGroupPath)
        {
            //Allow a short grace period for folder permissions to be established
            if (RainWorldInfo.LatestSetupPeriodReached < SetupPeriod.PreMods)
            {
                UtilityLogger.Log("Scheduling transfer dialog");

                string currentGroupPath = groupID.Properties.CurrentFolderPath;
                UtilityEvents.OnSetupPeriodReached += scheduledEvent;

                void scheduledEvent(SetupPeriodEventArgs e)
                {
                    if (e.CurrentPeriod < SetupPeriod.PreMods)
                        return;

                    bool pathChanged = !PathUtils.PathsAreEqual(currentGroupPath, groupID.Properties.CurrentFolderPath);
                    if (!pathChanged)
                    {
                        var dialog = new LogGroupTransferDialog(groupID, pendingGroupPath);
                        dialog.Show();
                    }
                    else
                    {
                        UtilityLogger.Log("Path already changed. Transfer no longer necessary.");
                    }
                    UtilityEvents.OnSetupPeriodReached -= scheduledEvent;
                }
                return;
            }
            //Dialog will run after Update frame completes. Although unlikely it is possible for the path to change by the time the dialog shows.
            var dialog = new LogGroupTransferDialog(groupID, pendingGroupPath);
            dialog.Show();
        }

        private static Vector2 calculateSize(LogGroupID groupID, string destinationPath)
        {
            if (groupID == null)
                throw new ArgumentNullException(nameof(groupID));

            const int DETAIL_WIDTH_BASE = 90; //The current size of unchanging label text
            const int MIN_WIDTH = 800;        //The minimum width of the dialog at any label size

            float height, width;

            height = 450.01f; //0.01 is important here (LABEL FIX)

            float longestDetail = Mathf.Max(LabelTest.GetWidth(groupID.Properties.CurrentFolderPath), LabelTest.GetWidth(destinationPath))
                                + DETAIL_WIDTH_BASE + (PADDING_X * 2); //Account for margins

            UtilityLogger.Logger.LogDebug($"Longest label is {longestDetail} units");
            width = Mathf.Max(longestDetail, MIN_WIDTH) + 0.01f; //Clamp to the minimum allowed value
            return new Vector2(width, height);
        }

        private void addDetails(ref Vector2 position)
        {
            string[] detailLabels =
            {
                //Group name
                $"GROUP: \'{groupID}\'",
                //Group path
                $"CURRENT:         \'{groupID.Properties.CurrentFolderPath}\'", //Spaces align with destination label
                //Destination path
                $"DESTINATION: \'{destinationPath}\'",
            };

            for (int i = 0; i < detailLabels.Length; i++)
            {
                float labelOffsetY = i > 0 ? LABEL_HEIGHT * (i + 1) : 0;

                Vector2 labelPosition = new Vector2(position.x + PADDING_X, position.y - labelOffsetY);
                MenuLabel label = new MenuLabel(this, dialogPage, detailLabels[i], labelPosition, Vector2.zero, false);

                label.label.alignment = FLabelAlignment.Left;
                dialogPage.subObjects.Add(label);
            }
            position.y -= LABEL_HEIGHT * detailLabels.Length + 1; //Account for label positions
        }

        private void addOptions(ref Vector2 position)
        {
            dialogOptions = new RadioButtonGroup(this, dialogPage, new Vector2(position.x + PADDING_X, position.y));

            //In all of these cases, the user path will be chosen as the new log group path even in the case the files/folders don't make it to the new path
            dialogOptions.AddOption(textWidth: 80f, Translate("Move entire group folder"), DialogOption.FOLDER_MOVE);
            dialogOptions.AddOption(textWidth: 80f, Translate("Move group files only"), DialogOption.FILE_MOVE);
            dialogOptions.AddOption(textWidth: 80f, Translate("Change group path only"), DialogOption.PATH_CHANGE);

            dialogOptions.SetInitial(DialogOption.FOLDER_MOVE);
            dialogPage.subObjects.Add(dialogOptions);
            position.y -= dialogOptions.Height; //Account for option positions
        }

        /// <summary>
        /// Initiates transfer strategy based on user selected option
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="message">Case-sensitive identifier</param>
        public override void Singal(MenuObject sender, string message)
        {
            if (message == DialogOption.OKAY || message == DialogOption.ACCEPT)
            {
                Singal(dialogOptions, dialogOptions.Selected.IDString);
                return;
            }

            UtilityLogger.Log("OPTION SELECTED: " + message);
            try
            {
                switch (message)
                {
                    case DialogOption.FOLDER_MOVE:
                        LogGroup.MoveFolder(groupID, destinationPath);
                        Dismiss();
                        return;
                    case DialogOption.FILE_MOVE:
                        LogGroup.MoveFiles(groupID, destinationPath);
                        Dismiss();
                        return;
                    case DialogOption.PATH_CHANGE:
                        groupID.Properties.ChangePath(destinationPath, applyToMembers: true);
                        Dismiss();
                        return;
                    default:
                        base.Singal(sender, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Failed to move group folder", ex);
            }
        }
    }
}
