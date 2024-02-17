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
            IL.Expedition.ChallengeOrganizer.RandomChallenge += ChallengeOrganizer_RandomChallenge;

            On.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
            IL.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
        }

        private static void ChallengeOrganizer_RandomChallenge(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            int outputIndex;

            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Challenge>(nameof(Challenge.Generate)));
            outputIndex = cursor.Index; //Record the index where Generate pushes something onto the stack
            cursor.GotoPrev(MoveType.After, x => x.MatchBlt(out _)); //This is before Generate is called
            
            ILLabel runGenerateAgain = cursor.MarkLabel();
            cursor.Index = outputIndex;
            cursor.Emit(OpCodes.Dup); 
            cursor.BranchStart(OpCodes.Brtrue); //Branch if Generate doesn't return null

            //A null return most likely means we cannot pick this Challenge type with current filter settings.
            //Remove Challenge type from list and try again
            cursor.EmitReference(FilterTarget);
            cursor.Emit(OpCodes.Ldloc_0); //Push list containing selectable challenges

            MethodInfo removeCall = typeof(List<Challenge>).GetMethod("Remove", new Type[] { typeof(Challenge) });
            cursor.Emit(OpCodes.Callvirt, removeCall);
            cursor.Emit(OpCodes.Br, runGenerateAgain);
            cursor.BranchFinish();
            
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
                //FilterTarget = null;
            }
        }

        private static void EchoChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            MethodInfo getCountMethod = typeof(List<string>).GetMethod("get_Count");

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to end of loop
            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //After end of loop

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
