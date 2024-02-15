using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static ExpeditionRegionSupport.ListUtils;

namespace ExpeditionRegionSupport.Challenges
{
    public static class ChallengeFilter
    {
        public static ChallengeFilterOptions CurrentFilter;

        public static void ApplyHooks()
        {
            On.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
            IL.Expedition.EchoChallenge.Generate += EchoChallenge_Generate;
        }

        private static Expedition.Challenge EchoChallenge_Generate(On.Expedition.EchoChallenge.orig_Generate orig, Expedition.EchoChallenge self)
        {
            Plugin.Logger.LogInfo("Generating Echo Challenge");
            return orig(self);
        }

        private static void EchoChallenge_Generate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.Before, x => x.MatchStloc(0)); //Move to list assignment
            cursor.Emit(OpCodes.Pop);
            cursor.EmitDelegate(createConstrainedEchoList); //Replace list with a ConstrainedList

            MethodInfo listAdd = typeof(List<string>).GetMethod("Add");
            MethodInfo newListAdd = typeof(ConstrainedList<string>).GetMethod("Add");

            //Replace Add call with Add call by a custom List
            cursor.GotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(listAdd));
            cursor.Remove();

            cursor.Emit(OpCodes.Call, newListAdd);
        }

        private static ConstrainedList<string> createConstrainedEchoList()
        {
            return new ConstrainedList<string>(regionCode =>
            {
                switch (CurrentFilter)
                {
                    case ChallengeFilterOptions.None:
                        return true;
                    case ChallengeFilterOptions.VisitedRegions:
                        return Plugin.RegionsVisited.Contains(regionCode);
                }
                return true;
            });
        }
    }

    public enum ChallengeFilterOptions
    {
        None,
        VisitedRegions
    }
}
