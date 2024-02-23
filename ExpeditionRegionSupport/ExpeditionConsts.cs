using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    public static class ExpeditionConsts
    {
        public static class Signals
        {
            //Challenge Select Interface
            public const string CHALLENGE_HIDDEN = "HIDDEN"; //Used with StartsWith
            public const string CHALLENGE_RANDOM = "RANDOM";
            public const string CHALLENGE_REPLACE = "CHA"; //Used with StartsWith
            public const string DESELECT_MISSION = "DESELECT"; //Also assigns challenges
            public const string ADD_SLOT = "PLUS";
            public const string REMOVE_SLOT = "MINUS";

            public const string POINTS = "POINTS";

            //Navigation
            public const string LEFT = "LEFT";
            public const string RIGHT = "RIGHT";

            //Other
            public const string OPEN_FILTER_DIALOG = "FILTER";
            public const string START_GAME = "BUTTON";

            //ExpeditionSettingsDialog
            public const string OPEN_SETTINGS_DIALOG = "SETTINGS";
            public const string OPEN_SPAWN_DIALOG = "OPEN_SPAWN_DIALOG"; //Opens random spawn dialog
            public const string RELOAD_MOD_FILES = "RELOAD_MOD_FILES"; //Triggers ModMerger to retrieve Expedition related files
            public const string RESTORE_DEFAULTS = "RESTORE_DEFAULTS"; //Triggers dialog to change all settings back to default values
        }
    }
}
