using LogUtils.Events;
using Menu;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public class FolderDialog
    {
        public const string DEFAULT_MESSAGE = "A folder move operation was initiated that requires your confirmation.";

        private static ProcessManager currentProcess => RainWorldInfo.RainWorld.processManager;

        private DialogConfirm dialog;

        public bool Result;

        internal FolderDialog(string message, Vector2 size)
        {
            dialog = new DialogConfirm(message, new Vector2(650, 175), currentProcess, Confirm, Cancel);
        }

        internal void Confirm()
        {
            Result = true;
        }

        internal void Cancel()
        {
            Result = false;
        }

        public void Show()
        {
            currentProcess.ShowDialog(dialog);
        }

        public static bool ConfirmMove(string sourcePath, string destinationPath, string message = DEFAULT_MESSAGE)
        {
            if (RainWorldInfo.LatestSetupPeriodReached < SetupPeriod.PreMods)
                throw new InvalidOperationException("Too early to activate dialog");

            message = FormatMessage(sourcePath, destinationPath, message);
            FolderDialog dialog = new FolderDialog(message, new Vector2(650, 175));

            dialog.Show();
            return dialog.Result;
        }

        /// <summary>
        /// Shows a confirmation dialog, and returns whether user has accepted, or canceled
        /// </summary>
        /// <param name="sourcePath">The source path that is pending a move operation</param>
        /// <param name="destinationPath">The destination path for a move operation</param>
        /// <param name="message">A message that describes the dialog</param>
        public static async Task<bool> ConfirmMoveAsync(string sourcePath, string destinationPath, string message = DEFAULT_MESSAGE)
        {
            if (RainWorldInfo.LatestSetupPeriodReached >= SetupPeriod.PreMods) //Late enough into init process to not have to schedule
            {
                message = FormatMessage(sourcePath, destinationPath, message);
                FolderDialog dialog = new FolderDialog(message, new Vector2(650, 175));

                dialog.Show();
                return dialog.Result;
            }

            bool result = false;
            await Task.Run(() =>
            {
                UtilityEvents.OnSetupPeriodReached += createDialogEvent;

                void createDialogEvent(SetupPeriodEventArgs e)
                {
                    if (e.CurrentPeriod < SetupPeriod.PreMods)
                        return;

                    message = FormatMessage(sourcePath, destinationPath, message);
                    FolderDialog dialog = new FolderDialog(message, new Vector2(650, 175));
                    
                    dialog.Show();
                    result = dialog.Result;
                    UtilityEvents.OnSetupPeriodReached -= createDialogEvent;
                }
            });
            return result;
        }

        internal static string FormatMessage(string sourcePath, string destinationPath, string message)
        {
            return message + "\n\n" +
                    "TARGET     : " + sourcePath + "\n" +
                    "DESTINATION: " + destinationPath;
        }
    }
}
