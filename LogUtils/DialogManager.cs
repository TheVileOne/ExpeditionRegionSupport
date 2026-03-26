using LogUtils.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils
{
    public class DialogManager
    {
        private DialogCollection<UtilityDialog> _dialogs;

        /// <summary>
        /// All <see cref="UtilityDialog"/> instances currently being shown or waiting to be shown
        /// </summary>
        public IEnumerable<UtilityDialog> Dialogs
        {
            get
            {
                var result = _dialogs;

                if (result != null)
                    return result;

                if (RainWorldInfo.IsRainWorldRunning)
                    result = new DialogCollection<UtilityDialog>(RainWorldInfo.RainWorld.processManager);

                _dialogs = result;
                return result ?? Enumerable.Empty<UtilityDialog>();
            }
        }

        public IEnumerable<UtilityDialog> DialogsInQueue
        {
            get
            {
                if (!RainWorldInfo.IsRainWorldRunning)
                    return Enumerable.Empty<UtilityDialog>();

                return RainWorldInfo.RainWorld.processManager._showDialogQueue.Where(data => data.Dialog is UtilityDialog).OfType<UtilityDialog>();
            }
        }

        /// <summary>
        /// Closes all dialogs that want to close
        /// </summary>
        public void ForceUpdate()
        {
            var pendingDismissal = Dialogs.Where(d => d.WantsToClose).ToArray();

            foreach (UtilityDialog dialog in pendingDismissal)
                dialog.Dismiss();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var dialog in Dialogs.Concat(DialogsInQueue))
                builder.AppendLine($"[{dialog.GetType()}]");

            return builder.ToString();
        }
    }
}
