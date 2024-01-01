using BepInEx;
using DependencyFlags = BepInEx.BepInDependency.DependencyFlags;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using MoreSlugcats;
using ExpeditionRegionSupport.Regions;
using UnityEngine;

namespace ExpeditionRegionSupport
{
    [BepInDependency("slime-cubed.slugbase", DependencyFlags.SoftDependency)]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "fluffball.expeditionregionsupport";
        public const string PLUGIN_NAME = "Expedition Region Support";
        public const string PLUGIN_VERSION = "0.9.2";

        public static new Debug.Logger Logger;

        public static bool SlugBaseEnabled;
        public WorldState ActiveWorldState;

        public void OnEnable()
        {
            Logger = new Debug.Logger(base.Logger);

            try
            {
                On.Menu.ExpeditionMenu.ctor += ExpeditionMenu_ctor;

                On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;

                On.Menu.ChallengeSelectPage.StartButton_OnPressDone += ChallengeSelectPage_StartButton_OnPressDone1;
                On.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
                IL.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
                IL.Menu.ChallengeSelectPage.StartButton_OnPressDone += ChallengeSelectPage_StartButton_OnPressDone;
                IL.SaveState.setDenPosition += SaveState_setDenPosition;
                
                On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void ChallengeSelectPage_StartButton_OnPressDone1(On.Menu.ChallengeSelectPage.orig_StartButton_OnPressDone orig, Menu.ChallengeSelectPage self, Menu.Remix.MixedUI.UIfocusable trigger)
        {
            ActiveWorldState = RegionUtils.GetWorldStateFromStoryRegions(ExpeditionData.slugcatPlayer, SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer));

            Logger.LogInfo("WS " + ActiveWorldState);

            orig(self, trigger);
        }

        private bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
        {
            if (ModManager.MSC && (/*self.room.world.name == "OE" ||*/ ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode
                                                                    && (ActiveWorldState & (WorldState.Vanilla | WorldState.Gourmand)) != 0
                                                                    && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand)))
                return true;

            return orig(self);
        }

        private void SaveState_setDenPosition(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Ret));
            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Ret)); //Move to second return statement
            cursor.GotoNext(MoveType.After, //Move to just after the check for ExpeditionMode
                x => x.MatchLdfld<SaveState>(nameof(SaveState.progression)),
                x => x.MatchLdfld<PlayerProgression>(nameof(PlayerProgression.rainWorld)),
                x => x.Match(OpCodes.Callvirt),
                x => x.Match(OpCodes.Brfalse_S));
            cursor.EmitDelegate(checkForExpeditionDen); //Check for null startingDen

            int cursorIndex = cursor.Index;
            cursor.GotoNext(MoveType.Before, //Move to within an if (startingDen != null) check
                x => x.Match(OpCodes.Ldstr),
                x => x.MatchLdsfld(typeof(ExpeditionData).GetField("startingDen")),
                x => x.Match(OpCodes.Call),
                x => x.Match(OpCodes.Call));

            ILLabel branchTarget = il.DefineLabel();
            cursor.MarkLabel(branchTarget); //Set the branch target

            //In original code a local variable called text was assigned the new den position when it should have assigned to startingDen.
            //Fix that behavior with these emits.
            cursor.GotoNext(MoveType.After, x => x.MatchStfld<SaveState>(nameof(SaveState.lastVanillaDen)));
            cursor.Emit(OpCodes.Ldarg_0); //Load SaveState (this) onto the stack.
            cursor.Emit<SaveState>(OpCodes.Ldfld, nameof(SaveState.lastVanillaDen)); //Push newly assigned field onto stack
            cursor.Emit(OpCodes.Stsfld, typeof(ExpeditionData).GetField("startingDen")); //Update startingDen with new den information

            //Finish branch logic
            //In original code, a new den is always processed even if it isn't used.
            //Fix this by branching past assignment when startingDen already exists.
            cursor.Index = cursorIndex;
            cursor.Emit(OpCodes.Brtrue, branchTarget);
        }

        /// <summary>
        /// This checks whether or not the den position is null. 
        /// </summary>
        private bool checkForExpeditionDen()
        {
            return ExpeditionData.startingDen != null;
        }

        /// <summary>
        /// This hook changes Expedition.startingDen to null instead of generating a new starting den.
        /// </summary>
        private void ChallengeSelectPage_StartButton_OnPressDone(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(x => x.MatchCallvirt<PlayerProgression>(nameof(PlayerProgression.WipeSaveState))); //Go to save wipe logic
            cursor.GotoNext(x => x.Match(OpCodes.Ldstr)); //Go to a String.Empty check after save wipe logic. (A point before the Ldarg used by the method call)
            cursor.GotoNext(MoveType.Before, x => x.Match(OpCodes.Ldarg_0)); //This should be where data is pushed onto the stack for the method call.

            int cursorIndex = cursor.Index;
            cursor.GotoNext(MoveType.Before, x => x.Match(OpCodes.Stsfld)); //The field we want to modify value assignment with
            
            ILLabel jumpLabel = il.DefineLabel();
            cursor.MarkLabel(jumpLabel); //Label after the method call.
            cursor.Emit(OpCodes.Pop); //This removes pointer information off the stack. Hook will fail without this.
            cursor.Emit(OpCodes.Ldnull); //Push a null reference onto the stack instead.

            cursor.Index = cursorIndex;
            //cursor.GotoPrev(MoveType.Before, x => x.Match(OpCodes.Ldarg_0)); //Go to before method arguments are loaded onto the stack.
            cursor.Emit(OpCodes.Br_S, jumpLabel); //Jump over method arguments and method call.
        }

        public static PlayerProgression StoredProgression;

        /// <summary>
        /// This hook stores save data needed to validate region spawning.
        /// </summary>
        private void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, Menu.ExpeditionMenu self, ProcessManager manager)
        {
            StoredProgression = manager.rainWorld.progression; //This data is going to be overwritten in the constructor, but this mod still needs access to it.
            orig(self, manager);
        }

        private bool hasProcessedRooms;

        /// <summary>
        /// The number of attempts made to find a valid room spawn in Expedition mode.
        /// </summary>
        private short attemptsToFindDenSpawn = 0;

        /// <summary>
        /// The maximum number of times a room can be rerolled
        /// </summary>
        private const short max_attempts_allowed = 3;

        private string ExpeditionGame_ExpeditionRandomStarts(On.Expedition.ExpeditionGame.orig_ExpeditionRandomStarts orig, RainWorld rainWorld, SlugcatStats.Name activeMenuSlugcat)
        {
            if (RegionSelector.Instance == null)
            {
                hasProcessedRooms = false;
                RegionSelector.Instance = new RegionSelector(activeMenuSlugcat);
            }
            else if (RegionSelector.Instance.ActiveSlugcat != activeMenuSlugcat)
            {
                hasProcessedRooms = false;
                RegionSelector.Instance.ActiveSlugcat = activeMenuSlugcat;
            }

            if (!hasProcessedRooms)
            {
                orig(rainWorld, activeMenuSlugcat);

                RegionSelector.Instance.RemoveEmptyRegions();
                hasProcessedRooms = true;
            }

            Plugin.Logger.LogInfo("LOGGING");

            RegionSelector.Instance.RegionsAvailable.ForEach(r => Logger.LogInfo(r.RegionCode + " " + r.AvailableRooms.Count));

            string spawnLocation = RegionSelector.Instance.RandomRoom();

            if (!RegionUtils.RoomExists(spawnLocation))
            {
                Logger.LogWarning($"Room {spawnLocation} does not exist");

                attemptsToFindDenSpawn++; //Tracks all attempts, not just reattempts
                if (spawnLocation == string.Empty || attemptsToFindDenSpawn >= max_attempts_allowed) //These is no hope for finding a new room
                {
                    Logger.LogWarning("Using fallback");
                    return SaveState.GetFinalFallbackShelter(activeMenuSlugcat);
                }

                return ExpeditionGame.ExpeditionRandomStarts(rainWorld, activeMenuSlugcat);
            }

            attemptsToFindDenSpawn = 0;
            return spawnLocation;
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            Logger = new Debug.Logger("ErsLog.txt", true); //Override BepInEx logger

            orig(self);

            SlugBaseEnabled = ModManager.ActiveMods.Exists(m => m.id == "slime-cubed.slugbase");
        }

        private void ExpeditionGame_ExpeditionRandomStarts(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Br)); //Move to within for loop
            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Ble)); //Move to just past the length check.

            int cursorIndex = cursor.Index;

            cursor.GotoNext(MoveType.Before, //Move to loop iteration logic
                x => x.MatchLdloc(10),
                x => x.MatchLdcI4(1),
                x => x.Match(OpCodes.Add));

            ILLabel loopReturnLabel = il.DefineLabel();
            cursor.MarkLabel(loopReturnLabel);
            cursor.Index = cursorIndex; //Go back and handle emits

            cursor.Emit(OpCodes.Ldloc, 4);
            cursor.Emit(OpCodes.Ldloc, 10);
            cursor.Emit(OpCodes.Ldelem_Ref); //Get value at current index in array

            cursor.EmitDelegate(handleRandomStarts); //Pass string stored at array[i] to hook
            cursor.Emit(OpCodes.Br_S, loopReturnLabel); //Shortcut loop

            cursor.GotoNext(MoveType.After, //Move to after loop ends
                x => x.Match(OpCodes.Ldlen),
                x => x.Match(OpCodes.Conv_I4),
                x => x.Match(OpCodes.Blt));

            cursor.Emit(OpCodes.Ldstr, string.Empty); //Return requires a string, but we don't need the return value anymore.
            cursor.Emit(OpCodes.Ret);
        }

        private void handleRandomStarts(string roomInfo)
        {
            Logger.LogDebug(roomInfo);
            RegionSelector.Instance.AddRoom(roomInfo);
        }

        public bool MineForGameComplete(SlugcatStats.Name name, RainWorld rainWorld)
        {
            if (!rainWorld.progression.IsThereASavedGame(name))
                return false;
            
            if (rainWorld.progression.currentSaveState != null && rainWorld.progression.currentSaveState.saveStateNumber == name)
                return rainWorld.progression.currentSaveState.deathPersistentSaveData.ascended || rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding;

            string[] progLinesFromMemory = rainWorld.progression.GetProgLinesFromMemory();
            if (progLinesFromMemory.Length == 0)
                return false;

            for (int i = 0; i < progLinesFromMemory.Length; i++)
            {
                string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
                if (array.Length == 2 && array[0] == "SAVE STATE" && array[1][21].ToString() == name.value)
                {
                    List<SaveStateMiner.Target> list = new List<SaveStateMiner.Target>();
                    list.Add(new SaveStateMiner.Target(">ASCENDED", null, "<dpA>", 20));
                    list.Add(new SaveStateMiner.Target(">ALTENDING", null, "<dpA>", 20));
                    List<SaveStateMiner.Result> list2 = SaveStateMiner.Mine(rainWorld, array[1], list);
                    bool flag = false;
                    bool flag2 = false;
                    for (int j = 0; j < list2.Count; j++)
                    {
                        string name_ = list2[j].name;
                        if (name_ == ">ASCENDED")
                        {
                            flag = true;
                        }
                        else if (name_ == ">ALTENDING")
                        {
                            flag2 = true;
                        }
                    }
                    return flag || flag2;
                }
            }
            return false;
        }

        private bool checkRegionRequirements(string text, SlugcatStats.Name slugcat, string[] storyRegions)
        {
            if (ExpeditionGame.lastRandomRegion == text) return false;

            bool checkFlag = false;

            if (storyRegions.Contains(text))
                checkFlag = true;
            else if (ModManager.MSC && (slugcat == SlugcatStats.Name.White || slugcat == SlugcatStats.Name.Yellow))
            {
                if (text == "OE" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
                {
                    checkFlag = true;
                }
                else if (text == "LC" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
                {
                    checkFlag = true;
                }
                else if (text == "MS" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
                {
                    checkFlag = true;
                }
            }
            else //Likely a custom region that has already been validated. Good to go!
                checkFlag = true;

            return checkFlag;
        }
    }
}