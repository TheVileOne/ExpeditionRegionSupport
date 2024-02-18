using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Expedition;
using ExpeditionRegionSupport.Regions;
using Extensions;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ExpeditionRegionSupport.Challenges
{
    public static class ChallengeFilter
    {
        public static ChallengeFilterOptions CurrentFilter;

        /// <summary>
        /// The Expedition challenge that the filter is handling, or is about to handle
        /// </summary>
        public static Challenge FilterTarget;

        public static bool HasFilter => CurrentFilter != ChallengeFilterOptions.None;

        public static void ApplyHooks()
        {
            try
            {
                IL.Expedition.ChallengeOrganizer.RandomChallenge += ChallengeOrganizer_RandomChallenge;

                On.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
                IL.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;

                On.Expedition.PearlDeliveryChallenge.Generate += PearlDeliveryChallenge_Generate;
                IL.Expedition.PearlDeliveryChallenge.Generate += PearlDeliveryChallenge_Generate;

                On.Expedition.NeuronDeliveryChallenge.Generate += NeuronDeliveryChallenge_Generate;

                On.Expedition.PearlHoardChallenge.Generate += PearlHoardChallenge_Generate;
                IL.Expedition.PearlHoardChallenge.Generate += PearlHoardChallenge_Generate;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static void ChallengeOrganizer_RandomChallenge(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Challenge>(nameof(Challenge.Generate)));
            int outputIndex = cursor.Index; //Record the index where Generate pushes something onto the stack

            cursor.GotoPrev(MoveType.After, x => x.MatchBlt(out _));

            ILLabel runGenerateAgain = cursor.MarkLabel();
            cursor.Index = outputIndex;
            cursor.Emit(OpCodes.Dup);
            cursor.BranchStart(OpCodes.Brtrue); //Null check Challenge gen - This means challenge type cannot be selected

            cursor.Emit(OpCodes.Ldloc_0); //Push list containing selectable challenges onto the stack
            cursor.EmitDelegate(onGenerationFailed);

            cursor.Emit(OpCodes.Br, runGenerateAgain);
            cursor.BranchFinish();
        }

        /// <summary>
        /// Handle when a challenge was unable to be selected
        /// </summary>
        private static void onGenerationFailed(List<Challenge> availableChallenges)
        {
            Plugin.Logger.LogInfo($"Challenge type {FilterTarget.ChallengeName()} could not be selected. Generating another");
            availableChallenges.Remove(FilterTarget);
        }

        private static Challenge EchoChallenge_Generate(On.Expedition.EchoChallenge.orig_Generate orig, EchoChallenge self)
        {
            FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (IndexOutOfRangeException)
            {
                //This will trigger if the list is empty, and some mod didn't check the list count after applying a mod-specific filter
                Plugin.Logger.LogWarning("Filter encountered an IndexOutOfRangeException");
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void EchoChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to end of loop
            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //After end of loop

            cursor.Emit(OpCodes.Ldloc_0); //Push list of region codes available for selection onto stack
            cursor.EmitDelegate(applyEchoChallengeFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_0); //Push list back on the stack to check its count
            applyEmptyListHandling(cursor);
        }

        private static Challenge PearlDeliveryChallenge_Generate(On.Expedition.PearlDeliveryChallenge.orig_Generate orig, PearlDeliveryChallenge self)
        {
            FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (IndexOutOfRangeException)
            {
                //This will trigger if the list is empty, and some mod didn't check the list count after applying a mod-specific filter
                Plugin.Logger.LogWarning("Filter encountered an IndexOutOfRangeException");
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void PearlDeliveryChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to end of loop
            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //After end of loop

            cursor.Emit(OpCodes.Ldloc_1); //Push list of region codes available for selection onto stack
            cursor.EmitDelegate(applyPearlDeliveryChallengeFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_1); //Push list back on the stack to check its count
            cursor.EmitDelegate<Func<List<string>, bool>>((allowedRegions) =>
            {
                if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                {
                    string deliveryRegion = RegionUtils.GetPearlDeliveryRegion(Plugin.ActiveWorldState);

                    //We cannot choose this challenge type if we haven't visited the delivery region yet
                    if (!allowedRegions.Contains(deliveryRegion))
                        return false;

                    return allowedRegions.Count > 0;
                }
                return true;
            });
            cursor.BranchStart(OpCodes.Brtrue); //Branch to main logic, or any additional mod-specific filters when list isn't empty
            cursor.Emit(OpCodes.Ldnull); //Return null to indicate that no challenges of the current type can be chosen
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }

        private static Challenge NeuronDeliveryChallenge_Generate(On.Expedition.NeuronDeliveryChallenge.orig_Generate orig, NeuronDeliveryChallenge self)
        {
            FilterTarget = self;

            //If player has not visited Shoreline, or Five Pebbles, this challenge type cannot be chosen
            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions && (!Plugin.RegionsVisited.Contains("SL") || !Plugin.RegionsVisited.Contains("SS")))
                return null;

            return orig(self);
        }

        private static Challenge PearlHoardChallenge_Generate(On.Expedition.PearlHoardChallenge.orig_Generate orig, PearlHoardChallenge self)
        {
            FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (IndexOutOfRangeException)
            {
                //This will trigger if the list is empty, and some mod didn't check the list count after applying a mod-specific filter
                Plugin.Logger.LogWarning("Filter encountered an IndexOutOfRangeException");
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void PearlHoardChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after array assignment
            cursor.BranchTo(OpCodes.Br, MoveType.Before, //Ignore check for HR. We need to use the list defined there
                x => x.MatchLdloc(1),
                x => x.Match(OpCodes.Call));
            cursor.GotoNext(MoveType.Before, x => x.MatchDup()); //Existing Dup removes HR
            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate(applyPearlHoardChallengeFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after array gets reformed
            cursor.Emit(OpCodes.Ldloc_1); //Push array on the stack
            cursor.EmitDelegate(convertToList); //Convert it to list for type compatibility
            applyEmptyListHandling(cursor);
        }

        private static List<string> convertToList(string[] array)
        {
            return array.ToList();
        }

        private static void applyEchoChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlDeliveryChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlHoardChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyEmptyListHandling(ILCursor cursor)
        {
            //Check that list on the stack has at least one item
            cursor.Emit(OpCodes.Call, typeof(List<string>).GetMethod("get_Count"));
            cursor.Emit(OpCodes.Ldc_I4_0);

            //Return null if list is empty
            cursor.BranchStart(OpCodes.Bgt);
            cursor.Emit(OpCodes.Ldnull);
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }
    }

    public enum ChallengeFilterOptions
    {
        None,
        VisitedRegions
    }
}
