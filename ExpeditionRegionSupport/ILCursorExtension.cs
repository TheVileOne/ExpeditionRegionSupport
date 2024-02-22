using BepInEx.Logging;
using ExpeditionRegionSupport;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class ILCursorExtension
    {
        /// <summary>
        /// ILLabels generated when BranchStart() is invoked awaiting an instruction to target
        /// </summary>
        public static readonly Stack<ILLabel> PendingBranchLabels = new Stack<ILLabel>();

        /// <summary>
        /// Generates an unconditional branch statement at current cursor index to the instruction returned by matchPredicates
        /// Cursor position will be moved to after branch target instruction on successful branch emit
        /// </summary>
        /// <param name="cursor">The cursor that will be handling the branching </param>
        /// <param name="matchPredicates">An array of IL match conditions that must all match a consecutive sequence of instructions</param>
        public static void BranchTo(this ILCursor cursor, params Func<Instruction, bool>[] matchPredicates)
        {
            cursor.BranchTo(OpCodes.Br, MoveType.After, matchPredicates);
        }

        /// <summary>
        /// Generates a conditional branch statement at current cursor index to the instruction returned by matchPredicates
        /// Cursor position will be moved to after branch target instruction on successful branch emit
        /// </summary>
        /// <param name="cursor">The cursor that will be handling the branching </param>
        /// <param name="branchCode">A branch related OpCode (OpCodes.Br, OpCodes.Brfalse, OpCodes.Bgt etc.)</param>
        /// <param name="matchPredicates">An array of IL match conditions that must all match a consecutive sequence of instructions</param>
        public static void BranchTo(this ILCursor cursor, OpCode branchCode, params Func<Instruction, bool>[] matchPredicates)
        {
            cursor.BranchTo(branchCode, MoveType.After, matchPredicates);
        }
        
        /// <summary>
        /// Generates a conditional branch statement at current cursor index to the instruction returned by matchPredicates
        /// Cursor position will be moved to before, or after branch target instruction depending on specified MoveType on successful branch emit
        /// </summary>
        /// <param name="cursor">The ILCursor that will be handling the branching </param>
        /// <param name="branchCode">A branch related OpCode (OpCodes.Br, OpCodes.Brfalse, OpCodes.Bgt etc.)</param>
        /// <param name="moveType">Specifies whether branch target will be before, or after matched instructions</param>
        /// <param name="matchPredicates">An array of IL match conditions that must all match a consecutive sequence of instructions</param>
        public static void BranchTo(this ILCursor cursor, OpCode branchCode, MoveType moveType, params Func<Instruction, bool>[] matchPredicates)
        {
            int cursorIndex = cursor.Index;

            //Move cursor to the branch target specified by match conditions. This code throws an exception if no match is found.
            cursor.GotoNext(moveType, matchPredicates);

            //Mark our branch target
            ILLabel branchTarget = cursor.DefineLabel();
            cursor.MarkLabel(branchTarget);

            //Go back and establish branch
            cursor.Index = cursorIndex;
            cursor.Emit(branchCode, branchTarget);

            //Set cursor index back to target instruction
            cursor.Goto(branchTarget.Target, moveType);
        }

        public static void BranchStart(this ILCursor cursor, OpCode branchCode)
        {
            //Define an ILLabel using the ILCursor for context
            ILLabel targetLabel = cursor.DefineLabel();

            //Emit instruction without a target. A target should be added through BranchFinish
            cursor.Emit(branchCode, targetLabel);

            //Store label reference until a target is ready to be applied
            PendingBranchLabels.Push(targetLabel);
        }

        public static void BranchFinish(this ILCursor cursor)
        {
            //Pop a pending ILLabel off the stack
            cursor.MarkLabel(PendingBranchLabels.Pop());
        }

        public static ILCursor EmitLog(this ILCursor cursor, string message, LogLevel logLevel = LogLevel.Info)
        {
            cursor.EmitReference(message);
            cursor.EmitReference(logLevel);
            cursor.EmitDelegate<Action<string, LogLevel>>(Plugin.Logger.Log);
            return cursor;
        }

    }
}
