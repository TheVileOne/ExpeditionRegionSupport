using Expedition;
using ExpeditionRegionSupport.Regions;
using Extensions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExpeditionRegionSupport.Filters
{
    public static partial class ChallengeFilterSettings
    {
        public static void ApplyHooks()
        {
            try
            {
                On.Expedition.ChallengeOrganizer.AssignChallenge += ChallengeOrganizer_AssignChallenge;
                IL.Expedition.ChallengeOrganizer.AssignChallenge += ChallengeOrganizer_AssignChallenge;

                On.Expedition.ChallengeOrganizer.RandomChallenge += ChallengeOrganizer_RandomChallenge;
                IL.Expedition.ChallengeOrganizer.RandomChallenge += ChallengeOrganizer_RandomChallenge;

                On.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
                IL.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;

                On.Expedition.PearlDeliveryChallenge.Generate += PearlDeliveryChallenge_Generate;
                IL.Expedition.PearlDeliveryChallenge.Generate += PearlDeliveryChallenge_Generate;

                On.Expedition.NeuronDeliveryChallenge.Generate += NeuronDeliveryChallenge_Generate;

                On.Expedition.PearlHoardChallenge.Generate += PearlHoardChallenge_Generate;
                IL.Expedition.PearlHoardChallenge.Generate += PearlHoardChallenge_Generate;

                On.Expedition.VistaChallenge.Generate += VistaChallenge_Generate;
                IL.Expedition.VistaChallenge.Generate += VistaChallenge_Generate;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static void ChallengeOrganizer_AssignChallenge(On.Expedition.ChallengeOrganizer.orig_AssignChallenge orig, int slot, bool hidden)
        {
            if (FailedToAssign) return;

            orig(slot, hidden);
        }

        private static Challenge ChallengeOrganizer_RandomChallenge(On.Expedition.ChallengeOrganizer.orig_RandomChallenge orig, bool hidden)
        {
            if (FailedToAssign) return null;

            return orig(hidden);
        }

        private static void ChallengeOrganizer_AssignChallenge(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after challenge is assigned
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.BranchStart(OpCodes.Brtrue); //Null check
            cursor.EmitDelegate<Action>(() => FailedToAssign = true);
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }

        private static void ChallengeOrganizer_RandomChallenge(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to end of loop
            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //After end of loop

            ILLabel runGenerateAgain = cursor.MarkLabel();

            cursor.Emit(OpCodes.Ldloc_0); //Push list of challenge types onto stack
            applyEmptyListHandling<Challenge>(cursor);

            while (cursor.TryGotoNext(MoveType.Before, x => x.MatchRet())) ; //Find the last return

            cursor.Emit(OpCodes.Dup);
            cursor.BranchStart(OpCodes.Brtrue); //Null check Challenge gen - This means challenge type cannot be selected

            cursor.Emit(OpCodes.Ldloc_0); //Push list containing selectable challenges onto the stack
            cursor.EmitDelegate(onGenerationFailed);

            cursor.Emit(OpCodes.Br, runGenerateAgain);
            cursor.BranchFinish();
        }

        #region Echo
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
            cursor.EmitDelegate(ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_0); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        #region Pearl Delivery
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
            cursor.EmitDelegate(ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_1); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        #region Neuron Delivery
        private static Challenge NeuronDeliveryChallenge_Generate(On.Expedition.NeuronDeliveryChallenge.orig_Generate orig, NeuronDeliveryChallenge self)
        {
            FilterTarget = self;

            if (!CheckConditions())
                return null;

            return orig(self);
        }
        #endregion

        #region Pearl Hoarding
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
            cursor.EmitDelegate(ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after array gets reformed
            cursor.Emit(OpCodes.Ldloc_1); //Push array on the stack
            cursor.EmitDelegate(convertToList); //Convert it to list for type compatibility
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        #region Vista
        private static Challenge VistaChallenge_Generate(On.Expedition.VistaChallenge.orig_Generate orig, VistaChallenge self)
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

        private static void VistaChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to end of loop
            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //After end of loop

            cursor.Emit(OpCodes.Ldloc_3); //Push list of region codes available for selection onto stack
            cursor.EmitDelegate(ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_3); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        private static List<string> convertToList(string[] array)
        {
            return array.ToList();
        }

        private static void applyEmptyListHandling<T>(ILCursor cursor)
        {
            //Check that list on the stack has at least one item
            cursor.Emit(OpCodes.Call, typeof(List<T>).GetMethod("get_Count"));
            cursor.Emit(OpCodes.Ldc_I4_0);

            //Return null if list is empty
            cursor.BranchStart(OpCodes.Bgt);
            cursor.Emit(OpCodes.Ldnull);
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }
    }
}
