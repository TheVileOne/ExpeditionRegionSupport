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

                #pragma warning disable IDE0055 // Fix formatting
                return signalText switch
                {
                    CHALLENGE_RANDOM     => nameof(CHALLENGE_RANDOM),
                    DESELECT_MISSION     => nameof(DESELECT_MISSION),
                    ADD_SLOT             => nameof(ADD_SLOT),
                    REMOVE_SLOT          => nameof(REMOVE_SLOT),
                    POINTS               => nameof(POINTS),
                    LEFT                 => nameof(LEFT),
                    RIGHT                => nameof(RIGHT),
                    OPEN_FILTER_DIALOG   => nameof(OPEN_FILTER_DIALOG),
                    START_GAME           => nameof(START_GAME),
                    OPEN_SETTINGS_DIALOG => nameof(OPEN_SETTINGS_DIALOG),
                    OPEN_SPAWN_DIALOG    => nameof(OPEN_SPAWN_DIALOG),
                    RELOAD_MOD_FILES     => nameof(RELOAD_MOD_FILES),
                    RESTORE_DEFAULTS     => nameof(RESTORE_DEFAULTS),
                    _ => signalText //Unrecognized signal
                };
                #pragma warning restore IDE0055 // Fix formatting
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
