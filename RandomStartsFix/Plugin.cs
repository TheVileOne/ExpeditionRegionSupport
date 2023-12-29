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
        public const string PLUGIN_GUID = "fluffball.expeditionregionsupport"; // This should be the same as the id in modinfo.json!
        public const string PLUGIN_NAME = "Expedition Region Support"; // This should be a human-readable version of your mod's name. This is used for log files and also displaying which mods get loaded. In general, it's a good idea to match this with your modinfo.json as well.
        public const string PLUGIN_VERSION = "0.9.2";

        public static new ManualLogSource Logger { get; private set; }

        public static List<SlugcatStats.Name> AvailableCampaigns = new List<SlugcatStats.Name>();

        public static bool SlugBaseEnabled;

        public void OnEnable()
        {
            Logger = base.Logger;

            try
            {
                On.Menu.ExpeditionMenu.ctor += ExpeditionMenu_ctor;

                On.MoreSlugcats.MSCRoomSpecificScript.DS_RIVSTARTcutscene.Update += DS_RIVSTARTcutscene_Update;
                On.Player.SuperHardSetPosition += Player_SuperHardSetPosition;
                On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;

                On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
                On.Player.CraftingResults += Player_CraftingResults;

                On.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
                IL.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;
                IL.Menu.ChallengeSelectPage.StartButton_OnPressDone += ChallengeSelectPage_StartButton_OnPressDone;
                IL.SaveState.setDenPosition += SaveState_setDenPosition;
                
                On.RainWorld.PostModsInit += RainWorld_PostModsInit;
                On.SaveState.setDenPosition += SaveState_setDenPosition;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            if (self.SlugCatClass == SlugcatStats.Name.Red)
                return self.input[0].y == 1 && self.CraftingResults() != null;

            return orig(self);
        }

        private AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            if (self.grasps.Length < 2 || self.SlugCatClass != SlugcatStats.Name.Red) //We need to be holding at least two things
                return orig(self);

            var craftingResult = CraftObject(self, self.grasps[0], self.grasps[1]);

            return craftingResult?.type;
        }

        private AbstractPhysicalObject GourmandCombos_CraftingResults(On.MoreSlugcats.GourmandCombos.orig_CraftingResults orig, PhysicalObject crafter, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if ((crafter as Player).SlugCatClass == SlugcatStats.Name.Red)
                return CraftObject(crafter as Player, graspA, graspB);

            return orig(crafter, graspA, graspB);
        }

        public AbstractPhysicalObject CraftObject(Player player, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if (player == null || graspA?.grabbed == null || graspB?.grabbed == null) return null;

            //Check grasps here
            if (player.SlugCatClass == SlugcatStats.Name.Red)
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Rock && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false);
                }
            }

            return null;
        }

        private bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
        {
            if (ModManager.MSC && (/*self.room.world.name == "OE" ||*/ ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand)))
                return true;

            return orig(self);
        }

        private void Player_SuperHardSetPosition(On.Player.orig_SuperHardSetPosition orig, Player self, UnityEngine.Vector2 pos)
        {
            /*if (self.tongue != null)
            {
                try
                {
                    if (self.tongue.Attached)
                    {
                        self.tongue.Release();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Logger.LogDebug("Tongue 1");
                }

                try
                {
                    self.tongue.pos = self.mainBodyChunk.pos;
                    self.tongue.lastPos = self.mainBodyChunk.lastPos;
                    self.tongue.rope.Reset(pos);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Logger.LogDebug("Tongue 2");
                }

                try
                {
                    foreach (PlayerGraphics.RopeSegment ropeSegment in (self.graphicsModule as PlayerGraphics).ropeSegments)
                    {
                        ropeSegment.pos = pos;
                        ropeSegment.lastPos = pos;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Logger.LogDebug("Tongue 3");
                    if ((self.graphicsModule as PlayerGraphics).ropeSegments == null)
                    {
                        Logger.LogError("Null ref found");
                    }
                }

            }

            try
            {
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    self.bodyChunks[i].HardSetPosition(pos);
                    for (int j = 0; j < 2; j++)
                    {
                        (self.graphicsModule as PlayerGraphics).drawPositions[i, j] = pos;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex);
                Logger.LogDebug("BODY CHUNKS");
            }

            try
            {
                foreach (BodyPart bodyPart in (self.graphicsModule as PlayerGraphics).bodyParts)
                {
                    bodyPart.pos = pos;
                    bodyPart.lastPos = pos;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                Logger.LogDebug("BODY PARTS");
            }*/

            try
            {
                orig(self, pos);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void DS_RIVSTARTcutscene_Update(On.MoreSlugcats.MSCRoomSpecificScript.DS_RIVSTARTcutscene.orig_Update orig, MSCRoomSpecificScript.DS_RIVSTARTcutscene self, bool eu)
        {
            try
            {
                orig(self, eu);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex);
                Logger.LogDebug("Player is " + self.room.game.FirstAlivePlayer);
            }
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

        private void SaveState_setDenPosition(On.SaveState.orig_setDenPosition orig, SaveState self)
        {
            Logger.LogInfo("Finding den spawn");
            if (ExpeditionData.startingDen != null)
                Logger.LogInfo("DEN: " + ExpeditionData.startingDen);
            else
                Logger.LogInfo("Starting den is NULL");

            orig(self);
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
            orig(self);

            SlugBaseEnabled = ModManager.ActiveMods.Exists(m => m.id == "slime-cubed.slugbase");

            string[] nameArray = ExtEnumBase.GetNames(typeof(SlugcatStats.Name));

            for (int i = 0; i < nameArray.Length; i++)
            {
                SlugcatStats.Name name = (SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), nameArray[i], true);

                Logger.LogInfo(name);
                AvailableCampaigns.Add(name);
            }

            try
            {
                On.MoreSlugcats.GourmandCombos.CraftingResults += GourmandCombos_CraftingResults;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
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

        bool regionSpecificFlag_ = false;
        bool customRegionAllowed_ = false;

        private bool processLine(/*string[] valueArray, int valueIndex*/ string value, RainWorld rainWorld)
        {
            //string value = valueArray[valueIndex];
            Logger.LogInfo(value);
            bool allowParse = true;

            if (value.StartsWith("#REGION_SPECIFIC"))
            {
                customRegionAllowed_ = false; //In case someone forgot to add an end line.
                regionSpecificFlag_ = true;
                //We need to check campaign completion status for these characters
                string[] restrictedToCharacters = value.Replace("#REGION_SPECIFIC", string.Empty).Split(',');

                foreach (string name in restrictedToCharacters)
                {
                    ExtEnumBase nameEnum;

                    if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), name, true, out nameEnum))
                    {
                        //Have we beaten the game with this slugcat
                        if (MineForGameComplete((SlugcatStats.Name)nameEnum, rainWorld))
                        {
                            customRegionAllowed_ = true;
                            break;
                        }
                    }

                    /*if (slugcatMatch(name.Trim(), slugcat))
                    {
                        customRegionAllowed = true;
                        break;
                    }*/
                }
                allowParse = false;
            }
            else if (regionSpecificFlag_)
            {
                if (value.StartsWith("#REGION_SPECIFIC_END") || value.StartsWith("#REGION_END") || value.StartsWith("#END"))
                {
                    //End custom region block
                    customRegionAllowed_ = false;
                    regionSpecificFlag_ = false;
                    allowParse = false;
                }
                else
                    allowParse = customRegionAllowed_;
            }
            return allowParse;
        }

        private static bool slugcatMatch(string name, SlugcatStats.Name slugcat)
        {
            return string.Equals(name.ToLower().Trim(), slugcat.ToString().ToLower().Trim());
        }

        /// <summary>
        /// This hook allows mod specific regions to add custom spawn data to be used in expedition mode.
        /// </summary>
        /*private string ExpeditionGame_ExpeditionRandomStarts(On.Expedition.ExpeditionGame.orig_ExpeditionRandomStarts orig, RainWorld rainWorld, SlugcatStats.Name slugcat)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
            string[] slugcatStoryRegions = SlugcatStats.getSlugcatStoryRegions(slugcat);
            if (File.Exists(AssetManager.ResolveFilePath("randomstarts.txt")))
            {
                bool regionSpecificFlag = false;
                bool customRegionAllowed = false;
                bool allowParse = false;

                string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("randomstarts.txt"));
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].StartsWith("//") && array[i].Length > 0)
                    {
                        if (array[i].StartsWith("#REGION_SPECIFIC"))
                        {
                            customRegionAllowed = false; //In case someone forgot to add an end line.
                            regionSpecificFlag = true;
                            //We need to check campaign completion status for these characters
                            string[] restrictedToCharacters = array[i].Replace("#REGION_SPECIFIC", string.Empty).Split(',');

                            foreach (string name in restrictedToCharacters)
                            {
                                ExtEnumBase nameEnum;

                                if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), name, true, out nameEnum))
                                {
                                    //Have we beaten the game with this slugcat
                                    if (MineForGameComplete((SlugcatStats.Name)nameEnum, rainWorld))
                                    {
                                        customRegionAllowed = true;
                                        break;
                                    }
                                }
                            }
                            allowParse = false;
                        }
                        else if (regionSpecificFlag)
                        {
                            if (array[i].StartsWith("#REGION_SPECIFIC_END") || array[i].StartsWith("#REGION_END") || array[i].StartsWith("#END"))
                            {
                                //End custom region block
                                customRegionAllowed = false;
                                regionSpecificFlag = false;
                                allowParse = false;
                            }
                            else
                                allowParse = customRegionAllowed;
                        }

                        if (allowParse)
                        {
                            string regionCode = Regex.Split(array[i], "_")[0]; //Room name format "region code_room name"

                            //Don't process the same region twice
                            if (checkRegionRequirements(regionCode, slugcat, slugcatStoryRegions))
                            {
                                //Hardcoded room check from MSC code
                                if (array[i] == "MS_S07" && ModManager.MSC && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
                                {
                                    continue;
                                }

                                if (!dictionary2.ContainsKey(regionCode))
                                    dictionary2.Add(regionCode, new List<string>());

                                dictionary2[regionCode].Add(array[i]);

                                if (dictionary2[regionCode].Contains(array[i]) && !dictionary.ContainsKey(regionCode))
                                    dictionary.Add(regionCode, ExpeditionGame.GetRegionWeight(regionCode));
                            }
                        }
                    }
                }
                Random random = new Random();
                int maxValue = dictionary.Values.Sum();
                int randomIndex = random.Next(0, maxValue);
                string key = dictionary.First(delegate (KeyValuePair<string, int> x)
                {
                    randomIndex -= x.Value;
                    return randomIndex < 0;
                }).Key;
                ExpeditionGame.lastRandomRegion = key;
                int num = (from list in dictionary2.Values
                           select list.Count).Sum();
                string text2 = dictionary2[key].ElementAt(UnityEngine.Random.Range(0, dictionary2[key].Count - 1));
                ExpLog.Log(string.Format("{0} | {1} valid regions for {2} with {3} possible dens", new object[]
                {
                    text2,
                    dictionary.Keys.Count,
                    slugcat.value,
                    num
                }));
                return text2;
            }
            return "SU_S01";
            //return orig(rainWorld, slugcat);
        }*/

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