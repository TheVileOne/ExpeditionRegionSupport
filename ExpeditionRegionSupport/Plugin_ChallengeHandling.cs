using Expedition;
using ExpeditionRegionSupport.Filters;
using ExpeditionRegionSupport.HookUtils;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    public partial class Plugin
    {
        private void ChallengeSelectPage_Singal(On.Menu.ChallengeSelectPage.orig_Singal orig, ChallengeSelectPage self, MenuObject sender, string signalText)
        {
            //Log any signals that get triggers as their const name
            Logger.LogInfo(ExpeditionConsts.Signals.GetName(signalText).Replace('_', ' '));

            orig(self, sender, signalText);
        }

        private void ChallengeSelectPage_Singal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //The order that these methods are called is important
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_REPLACE, false);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.DESELECT_MISSION, true);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_RANDOM, true);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.ADD_SLOT, false);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_HIDDEN, false);
        }

        private static ILWrapper challengeWrapper;
        private static ILWrapper challengeWrapperLoop;

        private static void assignWrappers()
        {
            if (challengeWrapper != null) return; //Only need to create wrappers once

            //Wrap assignment process with start/finish handlers
            challengeWrapper = new ILWrapper(
            before =>
            {
                before.Emit(OpCodes.Ldc_I4_1); //Push expected challenge request amount onto stack
                before.EmitDelegate(ChallengeAssignment.OnProcessStart);
            },
            after => after.EmitDelegate(ChallengeAssignment.OnProcessFinish));

            bool handled = false;
            challengeWrapperLoop = new ILWrapper(
            before =>
            {
                before.Emit(OpCodes.Dup);
                before.EmitDelegate<Action<int>>((requestAmount) => //Pass loop index limiter into delegate
                {
                    if (handled) return; //This delegate is forced to be part of the loop. Only handle once

                    handled = true;
                    ChallengeAssignment.OnProcessStart(requestAmount);
                });
            },
            after =>
            {
                after.EmitDelegate(() =>
                {
                    handled = false; //Reset flag for the next loop
                    ChallengeAssignment.OnProcessFinish();
                });
            });
        }

        /// <summary>
        /// Challenge assignment is handled in multiple places with the same emit logic
        /// </summary>
        private static void applyChallengeAssignmentIL(ILCursor cursor, string signalText, bool isLoop)
        {
            assignWrappers();

            if (signalText != null)
                cursor.GotoNext(MoveType.After, x => x.MatchLdstr(signalText));

            if (isLoop) //Process for a loop is slightly more complicated versus a single request
            {
                //The cursor will be moved to just after the index limit for the loop
                cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(ChallengeOrganizer).GetMethod("AssignChallenge")));
                cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to loop iterator
                cursor.GotoNext(MoveType.Before, x => x.MatchBlt(out _));
                challengeWrapperLoop.Apply(cursor);
            }
            else
            {
                cursor.GotoNext(MoveType.Before, x => x.MatchCall(typeof(ChallengeOrganizer).GetMethod("AssignChallenge")));
                challengeWrapper.Apply(cursor);
            }
        }

        private void ChallengeSelectPage_ctor(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            applyChallengeAssignmentIL(cursor, null, false);
        }

        private void ChallengeSelectPage_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            applyChallengeAssignmentIL(cursor, null, true);
        }
    }
}
