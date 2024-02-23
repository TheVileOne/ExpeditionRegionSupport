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
        private void ChallengeSelectPage_Singal(On.Menu.ChallengeSelectPage.orig_Singal orig, ChallengeSelectPage self, MenuObject sender, string message)
        {
            //Debug log messages for assignment signals
            if (message.StartsWith(ExpeditionConsts.Signals.CHALLENGE_REPLACE))
                Logger.LogInfo("REPLACE");
            else if (message == ExpeditionConsts.Signals.DESELECT_MISSION)
                Logger.LogInfo("DESELECT");
            else if (message == ExpeditionConsts.Signals.CHALLENGE_RANDOM)
                Logger.LogInfo("RANDOM");
            else if (message == ExpeditionConsts.Signals.REMOVE_SLOT)
                Logger.LogInfo("ONE LESS");
            else if (message == ExpeditionConsts.Signals.ADD_SLOT)
                Logger.LogInfo("ONE MORE");
            else if (message.StartsWith(ExpeditionConsts.Signals.CHALLENGE_HIDDEN))
                Logger.LogInfo("HIDDEN TOGGLE");

            orig(self, sender, message);
        }

        private void ChallengeSelectPage_Singal(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Wrap assignment process with start/finish handlers
            ILWrapper wrapper = new ILWrapper(
            before =>
            {
                before.Emit(OpCodes.Ldc_I4_1); //Push expected challenge request amount onto stack
                before.EmitDelegate(ChallengeAssignment.OnProcessStart);
            },
            after => after.EmitDelegate(ChallengeAssignment.OnProcessFinish));

            //The order that these methods are called is important
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_REPLACE, false, wrapper);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.DESELECT_MISSION, true, wrapper);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_RANDOM, true, wrapper);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.ADD_SLOT, false, wrapper);
            applyChallengeAssignmentIL(cursor, ExpeditionConsts.Signals.CHALLENGE_HIDDEN, false, wrapper);
        }

        /// <summary>
        /// Challenge assignment is handled in multiple places with the same emit logic
        /// </summary>
        private static void applyChallengeAssignmentIL(ILCursor cursor, string signalText, bool isLoop, ILWrapper wrapper)
        {
            cursor.GotoNext(MoveType.After, x => x.MatchLdstr(signalText));

            if (isLoop) //Process for a loop is slightly more complicated versus a single request
            {
                processOnLoopStart(cursor);

                cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //Move after loop
                cursor.EmitDelegate(ChallengeAssignment.OnProcessFinish);
            }
            else
            {
                cursor.GotoNext(MoveType.Before, x => x.MatchCall(typeof(ChallengeOrganizer).GetMethod("AssignChallenge")));
                wrapper.Apply(cursor);
            }
        }

        private void ChallengeSelectPage_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            processOnLoopStart(cursor);

            cursor.GotoNext(MoveType.After, x => x.MatchBlt(out _)); //Move after loop
            cursor.EmitDelegate(ChallengeAssignment.OnProcessFinish);
        }

        private static void processOnLoopStart(ILCursor cursor)
        {
            //This is within a loop. We need to get the number of loop iterations expected, which is after the loop's contents
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(ChallengeOrganizer).GetMethod("AssignChallenge")));
            cursor.GotoNext(MoveType.After, x => x.MatchAdd()); //Get closer to loop iterator
            cursor.GotoNext(MoveType.Before, x => x.MatchBlt(out _));

            bool handled = false;
            cursor.Emit(OpCodes.Dup);
            cursor.EmitDelegate<Action<int>>((i) => //Pass loop index limiter into delegate
            {
                if (handled) return; //This delegate is forced to be part of the loop. Only handle once

                handled = true;
                ChallengeAssignment.OnProcessStart(i);
            });
        }
    }
}
