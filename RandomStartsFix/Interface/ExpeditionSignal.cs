using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Interface
{
    public static class ExpeditionSignal
    {
        public const string OPEN_SPAWN_DIALOG = "OPEN_SPAWN_DIALOG"; //Opens random spawn dialog
        public const string RELOAD_MOD_FILES = "RELOAD_MOD_FILES"; //Triggers ModMerger to retrieve Expedition related files
        public const string RESTORE_DEFAULTS = "RESTORE_DEFAULTS"; //Triggers dialog to change all settings back to default values
    }
}
