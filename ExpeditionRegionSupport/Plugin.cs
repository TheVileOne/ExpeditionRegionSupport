using BepInEx;
using BepInEx.Logging;
using DependencyFlags = BepInEx.BepInDependency.DependencyFlags;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Expedition;
using ExpeditionRegionSupport.Filters;
using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Interface;
using ExpeditionRegionSupport.Regions;
using ExpeditionRegionSupport.Regions.Restrictions;
using Menu;
using MoreSlugcats;
using UnityEngine;
using Extensions;

namespace ExpeditionRegionSupport
{
    [BepInDependency("slime-cubed.slugbase", DependencyFlags.SoftDependency)]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "fluffball.expeditionregionsupport";
        public const string PLUGIN_NAME = "Expedition Region Support";
        public const string PLUGIN_VERSION = "0.9.55";

        public static bool DebugMode
        {
            get => ExpeditionData.devMode;
            set => ExpeditionData.devMode = value;
        }

        public static new Logging.Logger Logger;

        public static bool SlugBaseEnabled;
        public static WorldState ActiveWorldState;

        /// <summary>
        /// This is true when there is a reason to believe RegionsVisited is no longer accurate
        /// </summary>
        public static bool HasStaleRegionsCache = true;

        private static List<string> _regionsVisited;

        /// <summary>
        /// Cached list of regions visited by the currently selected slugcat in Expedition mode
        /// </summary>
        public static List<string> RegionsVisited
        {
            get
            {
                if (HasStaleRegionsCache)
                {
                    _regionsVisited = RegionUtils.GetVisitedRegions(ExpeditionData.slugcatPlayer);
                    HasStaleRegionsCache = false;
                }
                return _regionsVisited;
            }
        }

        private SimpleButton settingsButton;

        public void OnEnable()
        {
            Logger = new Logging.Logger(base.Logger);
            DebugMode = false;

            try
            {
                IL.Menu.ChallengeSelectPage.ctor += ChallengeSelectPage_ctor;
                On.Menu.ChallengeSelectPage.Singal += ChallengeSelectPage_Singal;
                IL.Menu.ChallengeSelectPage.Singal += ChallengeSelectPage_Singal;
                IL.Menu.ChallengeSelectPage.Update += ChallengeSelectPage_Update;
                On.Menu.ChallengeSelectPage.UpdateChallengeButtons += ChallengeSelectPage_UpdateChallengeButtons;
                IL.Menu.ChallengeSelectPage.UpdateChallengeButtons += ChallengeSelectPage_UpdateChallengeButtons;
                IL.Menu.CharacterSelectPage.Update += CharacterSelectPage_Update;

                //User Interface
                On.Menu.ExpeditionMenu.ctor += ExpeditionMenu_ctor;
                On.Menu.ExpeditionMenu.Update += ExpeditionMenu_Update;
                On.Menu.ExpeditionMenu.UpdatePage += ExpeditionMenu_UpdatePage;
                On.Menu.Menu.MenuColor += Menu_MenuColor;
                On.Menu.ButtonTemplate.InterpColor += ButtonTemplate_InterpColor;

                FilterDialogHooks.ApplyHooks();

                //User Input
                On.Menu.ExpeditionMenu.Singal += ExpeditionMenu_Singal;

                //CharacterSelect
                On.Menu.CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat;

                //Random Spawn hooks
                On.Menu.ChallengeSelectPage.StartGame += ChallengeSelectPage_StartGame;

                On.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
                IL.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;

                IL.SaveState.setDenPosition += SaveState_setDenPosition;

                //Misc.
                On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;
                On.RainWorld.PostModsInit += RainWorld_PostModsInit;

                //Allow communication with Log Manager
                Logging.Logger.ApplyHooks();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private HSLColor Menu_MenuColor(On.Menu.Menu.orig_MenuColor orig, Menu.Menu.MenuColors color)
        {
            if (color == ChallengeSlot.DISABLED_HOVER)
                return ChallengeSlot.DISABLE_HIGHLIGHT_COLOR;
            return orig(color);
        }

        private Color ButtonTemplate_InterpColor(On.Menu.ButtonTemplate.orig_InterpColor orig, ButtonTemplate self, float timeStacker, HSLColor baseColor)
        {
            var cwt = self.GetCWT();

            if (self.buttonBehav.greyedOut || (!cwt.IsChallengeSlot && self.inactive) || cwt.HighlightColor == Menu.Menu.MenuColors.White)
                return orig(self, timeStacker, baseColor);

            //Original code except using a cwt stored value
            float colorLerp = Mathf.Lerp(self.buttonBehav.lastCol, self.buttonBehav.col, timeStacker);
            float flashLerp = Mathf.Lerp(self.buttonBehav.lastFlash, self.buttonBehav.flash, timeStacker);

            return HSLColor.Lerp(baseColor, Menu.Menu.MenuColor(cwt.HighlightColor), Mathf.Max(colorLerp, flashLerp)).rgb;
        }

        private void CharacterSelectPage_UpdateSelectedSlugcat(On.Menu.CharacterSelectPage.orig_UpdateSelectedSlugcat orig, CharacterSelectPage self, int slugcatIndex)
        {
            SlugcatStats.Name lastSelected = ExpeditionData.slugcatPlayer;
            orig(self, slugcatIndex);

            if (lastSelected != ExpeditionData.slugcatPlayer)
            {
                ActiveWorldState = RegionUtils.GetWorldStateFromStoryRegions(ExpeditionData.slugcatPlayer);
                HasStaleRegionsCache = true;

                if (RegionFilterSettings.VisitedRegionsOnly.Value)
                    logRegionsVisited();
            }
        }

        private void ExpeditionMenu_UpdatePage(On.Menu.ExpeditionMenu.orig_UpdatePage orig, ExpeditionMenu self, int pageIndex)
        {
            orig(self, pageIndex);
            settingsButton.RemoveSprites();
            settingsButton.RemoveSubObject(settingsButton);
            settingsButton = createSettingsButton(self, self.pages[self.currentPage]);
            self.pages[self.currentPage].subObjects.Add(settingsButton);
        }

        /// <summary>
        /// This hook stores save data needed to validate region spawning, and sets a button for custom dialogue page.
        /// </summary>
        private void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, ExpeditionMenu self, ProcessManager manager)
        {
            ProgressionData.PlayerData.ProgressData = manager.rainWorld.progression; //This data is going to be overwritten in the constructor, but this mod still needs access to it.

            orig(self, manager);

            settingsButton = createSettingsButton(self, self.pages[1]);
            self.pages[1].subObjects.Add(settingsButton);

            ProgressionData.ExpeditionPlayerData.ProgressData = manager.rainWorld.progression;
        }

        private SimpleButton createSettingsButton(ExpeditionMenu menu, Page page)
        {
            float y = (menu.manager.rainWorld.options.ScreenSize.x != 1024f) ? 695f : 728f;

            Vector2 settingsButtonOrigPos = new Vector2(menu.rightAnchor - 150f, y - 40);

            return new SimpleButton(menu, page, menu.Translate("SETTINGS"), ExpeditionConsts.Signals.OPEN_SETTINGS_DIALOG, settingsButtonOrigPos, new Vector2(100f, 30f));
        }

        private void ExpeditionMenu_Update(On.Menu.ExpeditionMenu.orig_Update orig, ExpeditionMenu self)
        {
            orig(self);

            if (self.pagesMoving)
            {
                float movementAdjustedX = self.rightAnchor - (self.leftAnchor + 150f);
                float buttonOffsetY = 40 + (self.manager.rainWorld.options.ScreenSize.x != 1024f ? 695f : 728f);

                settingsButton.pos = new Vector2(self.manualButton.pos.x, self.manualButton.pos.y - 40);// - self.manualButton.page.pos;
                settingsButton.lastPos = new Vector2(self.manualButton.lastPos.x, self.manualButton.lastPos.y - 40);// - self.manualButton.page.lastPos;
                //settingsButton.page.pos = new Vector2(self.manualButton.page.pos.x, self.manualButton.page.pos.y - 40);
                //settingsButton.page.lastPos = new Vector2(self.manualButton.page.lastPos.x, self.manualButton.page.lastPos.y - 40);

                //settingsButton.pos = new Vector2(movementAdjustedX, settingsButton.pos.y - buttonOffsetY) - settingsButton.page.pos;
                //settingsButton.lastPos = new Vector2(movementAdjustedX, settingsButton.lastPos.y - buttonOffsetY) - settingsButton.page.lastPos;
            }
        }

        private void ExpeditionMenu_Singal(On.Menu.ExpeditionMenu.orig_Singal orig, ExpeditionMenu self, MenuObject sender, string message)
        {
            if (message == ExpeditionConsts.Signals.OPEN_SETTINGS_DIALOG)
            {
                try
                {
                    ExpeditionSettingsDialog settingsDialog = new ExpeditionSettingsDialog(self.manager, self.challengeSelect);
                    self.PlaySound(SoundID.MENU_Player_Join_Game);
                    self.manager.ShowDialog(settingsDialog);

                    settingsDialog.OnDialogClosed += SettingsDialog_OnDialogClosed;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }

            orig(self, sender, message);
        }

        public void SettingsDialog_OnDialogClosed(ExpeditionSettingsDialog sender)
        {
            ChallengeFilterSettings.CurrentFilter = FilterOptions.None;

            if (RegionFilterSettings.VisitedRegionsOnly.Value)
                ChallengeFilterSettings.CurrentFilter = FilterOptions.VisitedRegions;

            if (RegionFilterSettings.DetectShelterSpawns.Value)
            {

            }

            sender.OnDialogClosed -= SettingsDialog_OnDialogClosed;
        }

        private void ChallengeSelectPage_StartGame(On.Menu.ChallengeSelectPage.orig_StartGame orig, ChallengeSelectPage self)
        {
            ActiveWorldState = RegionUtils.GetWorldStateFromStoryRegions(ExpeditionData.slugcatPlayer);

            Logger.LogInfo("World State " + ActiveWorldState);
            orig(self);
        }

        private bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
        {
            if (ModManager.MSC
            && (/*self.room.world.name == "OE" ||*/ ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode
            && (ActiveWorldState & (WorldState.Vanilla | WorldState.Gourmand)) != 0
            && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand)))
            {
                return true;
            }

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

            Logger.LogInfo("LOGGING");

            RegionSelector.Instance.RegionsAvailable.ForEach(r => Logger.LogInfo(r.RegionCode + " " + r.AvailableRooms.Count));

            string spawnLocation = RegionSelector.Instance.RandomRoom();

            if (!RegionUtils.RoomExists(spawnLocation))
            {
                Logger.LogWarning($"Room {spawnLocation} does not exist");

                attemptsToFindDenSpawn++; //Tracks all attempts, not just reattempts
                if (spawnLocation == string.Empty || attemptsToFindDenSpawn >= max_attempts_allowed) //These is no hope for finding a new room
                {
                    Logger.LogWarning("Using fallback");
                    return SaveState.GetStoryDenPosition(activeMenuSlugcat, out _);
                }

                return ExpeditionGame.ExpeditionRandomStarts(rainWorld, activeMenuSlugcat);
            }

            attemptsToFindDenSpawn = 0;
            return spawnLocation;
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            Logger = new Logging.Logger("ErsLog", true); //Override BepInEx logger

            orig(self);

            ChallengeFilterHooks.ApplyHooks(); //This needs to be handled in PostModsInIt or Expedition.ChallengeTools breaks
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

        private static void logRegionsVisited()
        {
            var regionsVisited = RegionUtils.GetVisitedRegions(ExpeditionData.slugcatPlayer);

            Logger.LogInfo(regionsVisited.Count + " regions visited detected");
            foreach (string region in regionsVisited)
                Logger.LogInfo(region);
        }
    }
}