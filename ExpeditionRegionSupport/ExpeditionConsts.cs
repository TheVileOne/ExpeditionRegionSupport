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

            /// <summary>
            /// Retrieves the const name from its assigned signal text
            /// </summary>
            public static string GetName(string signalText)
            {
                if (signalText.StartsWith(CHALLENGE_REPLACE))
                    return nameof(CHALLENGE_REPLACE);
                else if (signalText.StartsWith(CHALLENGE_HIDDEN))
                    return nameof(CHALLENGE_HIDDEN);

                switch (signalText)
                {
                    case CHALLENGE_RANDOM:
                        return nameof(CHALLENGE_RANDOM);
                    case DESELECT_MISSION:
                        return nameof(DESELECT_MISSION);
                    case ADD_SLOT:
                        return nameof(ADD_SLOT);
                    case REMOVE_SLOT:
                        return nameof(REMOVE_SLOT);
                    case POINTS:
                        return nameof(POINTS);
                    case LEFT:
                        return nameof(LEFT);
                    case RIGHT:
                        return nameof(RIGHT);
                    case OPEN_FILTER_DIALOG:
                        return nameof(OPEN_FILTER_DIALOG);
                    case START_GAME:
                        return nameof(START_GAME);
                    case OPEN_SETTINGS_DIALOG:
                        return nameof(OPEN_SETTINGS_DIALOG);
                    case OPEN_SPAWN_DIALOG:
                        return nameof(OPEN_SPAWN_DIALOG);
                    case RELOAD_MOD_FILES:
                        return nameof(RELOAD_MOD_FILES);
                    case RESTORE_DEFAULTS:
                        return nameof(RESTORE_DEFAULTS);
                }

                //Unrecognized signal
                return signalText;
            }
        }

        public static class ChallengeNames
        {
            public const string ACHIEVEMENT = "AchievementChallenge";
            public const string CYCLE_SCORE = "CycleScoreChallenge";
            public const string ECHO = "EchoChallenge";
            public const string GLOBAL_SCORE = "GlobalScoreChallenge";
            public const string HUNT = "HuntChallenge";
            public const string ITEM_HOARD = "ItemHoardChallenge";
            public const string NEURON_DELIVERY = "NeuronDeliveryChallenge";
            public const string PEARL_DELIVERY = "PearlDeliveryChallenge";
            public const string PEARL_HOARD = "PearlHoardChallenge";
            public const string PIN = "PinChallenge";
            public const string VISTA = "VistaChallenge";
        }
    }
}
