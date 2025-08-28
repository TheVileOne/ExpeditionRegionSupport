using Expedition;
using ExpeditionRegionSupport.ExceptionHandling;
using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.HookUtils;
using ExpeditionRegionSupport.Regions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpeditionRegionSupport.Filters
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hooks should ignore capitalization rules")]
    public static class ChallengeFilterHooks
    {
        private static readonly ChallengeFilterExceptionHandler exceptionHandler = new ChallengeFilterExceptionHandler();

        private static readonly List<Hook> manualChallengeHooks = new List<Hook>();

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

        internal static void ApplyCustomChallengeHooks()
        {
#if RELEASE
            //Custom challenge support is not ready for release
            return;
#elif DEBUG
            Plugin.Logger.LogInfo("Applying Expedition Challenge hooks");

            Type baseType = typeof(Challenge);

            List<Type> hookGenerateRecord = new List<Type>();
            foreach (Challenge challenge in ChallengeOrganizer.availableChallengeTypes)
            {
                Type type = challenge.GetType();

                if (hookGenerateRecord.Contains(type)) continue; //Make sure hooks are only applied once per class type

                generateChallengeHooks(type);
                hookGenerateRecord.Add(type);

                while (type.BaseType != baseType && hookGenerateRecord.Contains(type.BaseType)) //This class is inheriting from a class that isn't the one packaged in Expedition
                {
                    generateChallengeHooks(type.BaseType);
                    type = type.BaseType; //Allow all inherited classes to have hooks
                    hookGenerateRecord.Add(type);
                }
            }

            manualChallengeHooks.ForEach(hook => hook.Apply());
#endif
        }

        private static void generateChallengeHooks(Type challengeType)
        {
            MethodInfo method = null;
            string hookTarget = nameof(Challenge.Generate);

            try
            {
                method = challengeType.GetMethod(hookTarget);
            }
            catch (AmbiguousMatchException)
            {
                Plugin.Logger.LogInfo("Overloaded method handled");
                method = Array.Find(challengeType.GetMethods(), m => m.Name == hookTarget && m.GetParameters().Length == 0); //Find the version without parameters
            }

            if (method != null)
                manualChallengeHooks.Add(new Hook(method, challengeGenerateHook));
        }

        private static Challenge challengeGenerateHook(Func<Challenge, Challenge> orig, Challenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            //TODO: This interface is broken. We need a reflection solution instead
            IRegionChallenge customRegionChallenge = self as IRegionChallenge;

            if (customRegionChallenge != null)
            {
                try
                {
                    applyCustomFilter(customRegionChallenge);
                }
                catch (Exception ex)
                {
                    exceptionHandler.HandleException(ChallengeFilterSettings.FilterTarget, ex);
                    return null; //Return null to indicate that no challenges of the current type can be chosen
                }
            }
            return orig(self);
        }

        private static void applyCustomFilter(IRegionChallenge customChallenge)
        {
            FilterApplicator<string> customChallengeFilter = new FilterApplicator<string>(customChallenge.ApplicableRegions);

            List<string> availableRegions = RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);

            //TODO: This filter needs to be removed at the end of the process
            //Remove all applicable regions that are not also part of the active filter cache
            customChallengeFilter.ItemsToRemove.AddRange(customChallenge.ApplicableRegions.Except(availableRegions));
            customChallengeFilter.Apply();
        }

        private static void ChallengeOrganizer_AssignChallenge(On.Expedition.ChallengeOrganizer.orig_AssignChallenge orig, int slot, bool hidden)
        {
            ChallengeAssignment.AssignSlot(slot);

            if (!ChallengeAssignment.Aborted)
            {
                ChallengeAssignment.OnAssignStart();
                orig(slot, hidden);
                ChallengeAssignment.OnAssignFinish();
            }
        }

        private static void ChallengeOrganizer_AssignChallenge(ILContext il)
        {
            limitDuplicationCheck(il);
            attachAssignmentEvents(il);
        }

        /// <summary>
        /// Avoids checking the entire list of slot challenges when determining if a duplication is present.
        /// Only up the first playable slot, or the last processed request, whichever is greater is necessary.
        /// </summary>
        private static void limitDuplicationCheck(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Challenge>(nameof(Challenge.Duplicable))); //Move to Duplication check
            cursor.GotoForLoopLimit(); //Access the index limiter

            //Replace it with a new value
            cursor.Emit(OpCodes.Pop);
            cursor.EmitDelegate(() => //At this point, the CurrentRequest is still being processed and not stored in Requests
            {
                //Limits check range to only processed challenges - only applies during a full process
                if (ChallengeAssignment.ChallengesRequested > 1 && ChallengeAssignment.FullProcess)
                    return Math.Max(ChallengeAssignment.CurrentRequest.Slot, ChallengeSlot.FirstPlayableSlot());

                return ChallengeSlot.SlotChallenges.Count; //Any other situation should check the entire challenge list for duplicates
            });
        }

        /// <summary>
        /// Establishes methods for processing fail/success events during the challenge assignment process
        /// </summary>
        private static void attachAssignmentEvents(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            handleAbortConditions(cursor);

            //Handle invalid challenges
            int failIndex = 0;

            //Each time the loop index is increased, track the index, and pass the index and the challenge into a method
            while (cursor.TryGotoNext(MoveType.After, x => x.MatchAdd(), x => x.MatchStloc(0)))
            {
                cursor.Emit(OpCodes.Ldloc_1); //Challenge
                cursor.EmitReference(failIndex); //Index at this stage in loop
                cursor.EmitDelegate(ChallengeAssignment.OnChallengeRejected);
                failIndex++;
            }

            //Handle valid challenges
            cursor.GotoNext(MoveType.Before, x => x.MatchRet());
            cursor.Emit(OpCodes.Ldloc_1); //Challenge
            cursor.EmitDelegate(ChallengeAssignment.OnChallengeAccepted);
        }

        private static void handleAbortConditions(ILCursor cursor)
        {
            cursor.GotoNext(MoveType.After,
                x => x.MatchBlt(out _),
                x => x.Match(OpCodes.Ldstr)); //This is the Too many attempts string that logs
            cursor.GotoNext(MoveType.Before, x => x.MatchRet()); //Move to just before the return
            cursor.EmitDelegate<Action>(() => ChallengeAssignment.Aborted = true); //Notify that no more changes should be assigned

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after challenge is assigned
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.BranchStart(OpCodes.Brtrue); //Null check
            cursor.EmitDelegate<Action>(() => ChallengeAssignment.Aborted = true); //Notify that an entire challenge group has failed to return valid results
            cursor.Emit(OpCodes.Ret);
            cursor.BranchFinish();
        }

        private static Challenge ChallengeOrganizer_RandomChallenge(On.Expedition.ChallengeOrganizer.orig_RandomChallenge orig, bool hidden)
        {
            ChallengeAssignment.OnChallengeSelect();

            try
            {
                return orig(hidden);
            }
            finally
            {
                ChallengeAssignment.OnChallengeSelectFinish();
            }
        }

        private static void ChallengeOrganizer_RandomChallenge(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoAfterForLoop();

            ILLabel runGenerateAgain = cursor.MarkLabel();

            cursor.Emit(OpCodes.Ldloc_0); //Push list of challenge types onto stack
            applyEmptyListHandling<Challenge>(cursor);

            while (cursor.TryGotoNext(MoveType.Before, x => x.MatchRet())) ; //Find the last return

            cursor.Emit(OpCodes.Dup);
            cursor.BranchStart(OpCodes.Brtrue); //Null check Challenge gen - This means challenge type cannot be selected

            cursor.Emit(OpCodes.Ldloc_0); //Push list of filtered options onto stack
            cursor.EmitDelegate((List<Challenge> list) =>
            {
                list.Remove(ChallengeFilterSettings.FilterTarget);
                ChallengeAssignment.OnGenerationFailed();
            });
            cursor.Emit(OpCodes.Br, runGenerateAgain);
            cursor.BranchFinish();
        }

        #region Echo
        private static Challenge EchoChallenge_Generate(On.Expedition.EchoChallenge.orig_Generate orig, EchoChallenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(ChallengeFilterSettings.FilterTarget, ex);
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void EchoChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(0));
            cursor.EmitDelegate(processFilterEcho); //Replace it with filtered list
            cursor.Emit(OpCodes.Stloc_0);

            cursor.BranchStart(OpCodes.Br); //Original filter logic is bad, and has been replaced
            cursor.GotoAfterForLoop();
            cursor.BranchFinish();

            //Apply filter logic

            cursor.Emit(OpCodes.Ldloc_0); //Push list of region codes available for selection onto stack
            cursor.EmitDelegate(ChallengeFilterSettings.ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_0); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }

        private static List<string> processFilterEcho()
        {
            List<string> availableRegions = RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);
            List<string> availableEchoRegions = ChallengeUtils.GetApplicableEchoRegions(ExpeditionData.slugcatPlayer);

            return Filter.GetMatches(availableEchoRegions, availableRegions).ToList();
        }

        #endregion

        #region Pearl Delivery
        private static Challenge PearlDeliveryChallenge_Generate(On.Expedition.PearlDeliveryChallenge.orig_Generate orig, PearlDeliveryChallenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(ChallengeFilterSettings.FilterTarget, ex);
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void PearlDeliveryChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Go to after list is instantiated
            cursor.EmitDelegate(() => //Replace reference with mod managed regions list
            {
                return RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);
            });
            cursor.Emit(OpCodes.Stloc_1);

            cursor.BranchStart(OpCodes.Br); //The filter logic in the loop is no longer necessary
            cursor.GotoAfterForLoop();
            cursor.BranchFinish();

            //Apply filter logic

            cursor.Emit(OpCodes.Ldloc_1); //Push list of region codes available for selection onto stack
            cursor.EmitDelegate(ChallengeFilterSettings.ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.Emit(OpCodes.Ldloc_1); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        #region Neuron Delivery
        private static Challenge NeuronDeliveryChallenge_Generate(On.Expedition.NeuronDeliveryChallenge.orig_Generate orig, NeuronDeliveryChallenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            if (!ChallengeFilterSettings.CheckConditions())
                return null;

            return orig(self);
        }
        #endregion

        #region Pearl Hoarding
        private static Challenge PearlHoardChallenge_Generate(On.Expedition.PearlHoardChallenge.orig_Generate orig, PearlHoardChallenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(ChallengeFilterSettings.FilterTarget, ex);
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void PearlHoardChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(1)); //Move to after regions array assignment
            cursor.EmitDelegate(() => //Replace reference with mod managed regions list
            {
                return RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);
            });
            cursor.Emit(OpCodes.Stloc_1);

            //Apply filter logic

            cursor.Emit(OpCodes.Ldloc_1); //Push list of region codes onto stack to apply filter
            cursor.EmitDelegate(ChallengeFilterSettings.ApplyFilter);

            //Handle list post filter. An empty list will throw an exception

            cursor.GotoNext(MoveType.After, x => x.MatchPop()); //This matches after the HR remove logic on the stack
            cursor.Emit(OpCodes.Ldloc_1); //Push list back on the stack to check its count
            applyEmptyListHandling<string>(cursor);
        }
        #endregion

        #region Vista
        private static Challenge VistaChallenge_Generate(On.Expedition.VistaChallenge.orig_Generate orig, VistaChallenge self)
        {
            ChallengeFilterSettings.FilterTarget = self;

            try
            {
                return orig(self);
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(ChallengeFilterSettings.FilterTarget, ex);
                return null; //Return null to indicate that no challenges of the current type can be chosen
            }
        }

        private static void VistaChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(3)); //Go to after list is created
            cursor.Emit(OpCodes.Ldloc_3); //Push it back onto the stack
            cursor.EmitDelegate(populateVistasFromCache); //Send it to method for population

            cursor.BranchStart(OpCodes.Brtrue); //Branch over processing logic if list was populated from the cache
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(SlugcatStats).GetMethod("SlugcatStoryRegions")));
            cursor.Emit(OpCodes.Pop);
            cursor.EmitDelegate(() => //Replace reference with mod managed regions list
            {
                return RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);
            });

            //Apply filter logic

            cursor.GotoNext(MoveType.After, x => x.MatchEndfinally());

            ILLabel leaveTarget = null;
            cursor.GotoNext(x => x.MatchLeaveS(out leaveTarget)); //The break target of a foreach loop
            cursor.GotoLabel(leaveTarget);

            //This emit will set the new leave target
            cursor.Emit(OpCodes.Ldloc_3); //Push list of vista locations available for selection onto stack
            cursor.EmitDelegate(assignCache);
            cursor.BranchFinish();

            //Handle list post filter. An empty list will throw an exception
            cursor.Emit(OpCodes.Ldloc_3); //Push list back on the stack to check its count
            applyEmptyListHandling<ValueTuple<string, string>>(cursor);
        }

        private static List<ValueTuple<string, string>> allowedVistasCache;

        private static bool populateVistasFromCache(List<ValueTuple<string, string>> list)
        {
            if (allowedVistasCache != null)
            {
                Plugin.Logger.LogInfo("Vista location cache present");
                list.AddRange(allowedVistasCache);
                return true;
            }
            return false;
        }

        private static void assignCache(List<ValueTuple<string, string>> list)
        {
            Plugin.Logger.LogInfo("Caching vista locations");
            allowedVistasCache = list;

            ChallengeAssignment.HandleOnProcessComplete += clearAllowedVistas;
        }

        private static void clearAllowedVistas()
        {
            Plugin.Logger.LogInfo("Clearing vista locations");
            allowedVistasCache = null;
        }
        #endregion

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
