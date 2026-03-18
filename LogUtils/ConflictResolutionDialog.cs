using LogUtils.Events;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class ConflictResolutionDialog : UtilityDialog
    {
        internal const string DESCRIPTION = "Conflict resolution test";

        private readonly MergeEventHandler events;
        private readonly MergeHistory history;
        private readonly ConflictResolutionHandler handler;

        public ConflictResolutionDialog(MergeHistory history, MergeEventHandler mergeEvents) : base(DESCRIPTION, new Vector2(200, 200), RainWorldInfo.RainWorld.processManager)
        {
            this.events = mergeEvents;
            this.handler = new ConflictResolutionHandler(history.Conflicts);
            this.history = history;
        }

        /// <summary>
        /// Indicates that there are pending, or active dialogs for this type
        /// </summary>
        public static bool HasAnyDialogs => UtilityCore.CurrentDialogs.Where(dialog => dialog.IsActive || dialog.IsPending).ContainsType<ConflictResolutionDialog>();

        public static void ShowDialog(MergeHistory history, MergeEventHandler mergeEvents)
        {
            //It is important that folder permissions be assigned before the user interacts with this dialog. Dialog can only run after mod init completes
            //and game will continue to process update frames while dialog is running giving a window for enabled mods to assign folder permissions.
            if (!MustBeScheduled)
            {
                UtilityDialog dialog = new ConflictResolutionDialog(history, mergeEvents);
                dialog.Show();
                return;
            }

            UtilityLogger.Log("Scheduling merge conflict resolution");
            UtilityEvents.OnSetupPeriodReached += scheduledEvent;

            void scheduledEvent(SetupPeriodEventArgs e)
            {
                if (e.CurrentPeriod < SetupPeriod.PreMods)
                    return;

                UtilityDialog dialog = new ConflictResolutionDialog(history, mergeEvents);
                dialog.Show();
                UtilityEvents.OnSetupPeriodReached -= scheduledEvent;
            }
        }
    }
}
