using LogUtils.Enums;
using LogUtils.Helpers;
using Menu;
using System;
using UnityEngine;
using DialogOption = LogUtils.UtilityConsts.DialogOption;

namespace LogUtils
{
    public class LogGroupTransferDialog : Dialog
    {
        internal const string DESCRIPTION = "A log group change has been detected that requires your input.";

        internal const float PADDING = 14f;

        private static ProcessManager currentProcess => RainWorldInfo.RainWorld.processManager;

        private readonly LogGroupID groupID;
        private readonly string destinationPath;

        private RadioButtonGroup dialogOptions;

        public LogGroupTransferDialog(LogGroupID groupID, string destinationPath) : base(DESCRIPTION, currentProcess)
        {
            this.groupID = groupID;
            this.destinationPath = destinationPath;

            Vector2 currentPosition = new Vector2(descriptionLabel.pos.x, descriptionLabel.pos.y + descriptionLabel.size.y);

            currentPosition.y += PADDING * 2;
            addDetails(ref currentPosition);

            currentPosition.y += PADDING;
            addOptions(ref currentPosition);

            currentPosition.y += PADDING;
            SimpleButton acceptButton = new SimpleButton(this, dialogPage, Translate("Accept"), DialogOption.ACCEPT,  currentPosition, new Vector2(110f, 30f));
            dialogPage.subObjects.Add(acceptButton);
        }

        private void addDetails(ref Vector2 position)
        {
            Vector2 labelSize = new Vector2(180f, 24f);

            string[] detailLabels =
            {
                //Group name
                "GROUP NAME      : " + groupID.Value,
                //Group path
                "CURRENT PATH    : " + groupID.Properties.CurrentFolderPath,
                //Destination path
                "DESTINATION PATH: " + destinationPath
            };

            for (int i = 0; i < detailLabels.Length; i++)
            {
                Vector2 labelPosition = new Vector2(position.x, position.y + (labelSize.y * i));
                dialogPage.subObjects.Add(new MenuLabel(this, dialogPage, detailLabels[i], labelPosition, labelSize, false));
            }
            position.y += labelSize.y * detailLabels.Length - 1; //Account for label positions
        }

        private void addOptions(ref Vector2 position)
        {
            dialogOptions = new RadioButtonGroup(this, dialogPage, position);

            //In all of these cases, the user path will be chosen as the new log group path even in the case the files/folders don't make it to the new path
            dialogOptions.AddOption(textWidth: 80f, Translate("Move entire group folder"), DialogOption.FOLDER_MOVE);
            dialogOptions.AddOption(textWidth: 80f, Translate("Move group files only"), DialogOption.FILE_MOVE);
            dialogOptions.AddOption(textWidth: 80f, Translate("Change group path only"), DialogOption.PATH_CHANGE);

            dialogOptions.SetInitial(DialogOption.FOLDER_MOVE);
            dialogPage.subObjects.Add(dialogOptions);
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

        public void Show()
        {
            currentProcess.ShowDialog(this);
        }

        public void Dismiss()
        {
            currentProcess.StopSideProcess(this);
        }
    }
}
