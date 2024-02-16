using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Expedition;
using Extensions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static ExpeditionRegionSupport.ListUtils;

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
            On.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
            IL.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
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
            finally
            {
                FilterTarget = null;
            }
        }

        private static void EchoChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            MethodInfo getCountMethod = typeof(List<string>).GetMethod("get_Count");

            //Apply filter logic

            cursor.GotoNext(MoveType.After, //Move us closer to loop process code
                x => x.MatchAdd(),
                x => x.MatchStloc(2),
                x => x.MatchLdloc(2));
            cursor.GotoNext(MoveType.After, //Move to after loop finishes
                x => x.MatchCallOrCallvirt(getCountMethod),
                x => x.Match(OpCodes.Blt));
            cursor.Emit(OpCodes.Ldloc_0); //Push list of echo region options on the stack
            cursor.EmitDelegate(applyEchoChallengeFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_0); //Push list back on the stack to check its count
            cursor.Emit(OpCodes.Call, getCountMethod);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.BranchStart(OpCodes.Bgt); //Branch to main logic, or any additional mod-specific filters when list isn't empty
            cursor.Emit(OpCodes.Ldnull); //Return null to indicate that no challenges of the current type can be chosen
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }

        private static void applyEchoChallengeFilter(List<string> echoRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                echoRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }
    }

    public enum ChallengeFilterOptions
    {
        None,
        VisitedRegions
    }
}
