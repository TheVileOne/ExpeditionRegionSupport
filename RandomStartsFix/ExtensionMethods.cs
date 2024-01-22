using ExpeditionRegionSupport.Interface;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    public static class ExtensionMethods
    {
        //FilterDialog

        public class FilterDialogCWT
        {
            public Action<FilterDialog> RunOnNextUpdate;
            public ScrollablePage Page;
            public FilterOptions Options;
        }

        public static readonly ConditionalWeakTable<FilterDialog, FilterDialogCWT> filterDialogCWT = new();

        public static FilterDialogCWT GetCWT(this FilterDialog self) => filterDialogCWT.GetValue(self, _ => new());

        //MenuObject

        public static void SetAlpha(this MenuObject self, float alpha)
        {
            self.Container.alpha = alpha;

            foreach (MenuObject child in self.subObjects)
                child.SetAlpha(alpha);
        }

        //FContainer

        public class FContainerCWT
        {
            public bool EventTrackingEnabled;
            public List<FNode> AddedList = null;
            public void HandleOnAdded(FNode node)
            {
                if (AddedList == null)

                    AddedList.Add(node);
            }
        }

        /// <summary>
        /// Take all child FNodes in one container and place them in another container
        /// </summary>
        public static void MoveChildrenToNewContainer(this FContainer currentParent, FContainer newParent)
        {
            List<FNode> newChildList = new List<FNode>(currentParent._childNodes);

            currentParent.RemoveAllChildren();
            newChildList.ForEach(newParent.AddChild);
        }

        public static readonly ConditionalWeakTable<FContainer, FContainerCWT> fContainerCWT = new();

        public static FContainerCWT GetCWT(this FContainer self) => fContainerCWT.GetValue(self, _ => new());
    }

    public static class ILCursorExtension
    {
        public static void BranchTo(this ILCursor cursor, params Func<Instruction, bool>[] matchPredicates)
        {
            int cursorIndex = cursor.Index;

            cursor.GotoNext(MoveType.After, matchPredicates);

            //Mark our branch target
            ILLabel branchTarget = cursor.DefineLabel();
            cursor.MarkLabel(branchTarget);

            //Go back and establish branch
            cursor.Index = cursorIndex;
            cursor.Emit(OpCodes.Br, branchTarget);
            cursor.GotoLabel(branchTarget);
        }

        /// <summary>
        /// Begin at current cursor index, run a match search, define a label if found, and finally emit a branch statement
        /// </summary>
        /// <param name="cursor">Used to setup the hook</param>
        /// <param name="moveType">The position of the branch label relative to match result</param>
        /// <param name="branchCode">The type of branch instruction to emit</param>
        /// <param name="matchPredicates">Series of predicates used to find an IL match</param>
        public static bool BranchOver(this ILCursor cursor, MoveType moveType, OpCode branchCode, params Func<Instruction, bool>[] matchPredicates)
        {
            //Create a new cursor to avoid interfering with main cursor's position
            ILCursor branchCursor = new ILCursor(cursor);

            int cursorIndex = branchCursor.Index;

            ILLabel branchTarget = branchCursor.FindBranchTarget(moveType, matchPredicates);

            if (branchTarget != null)
            {
                //Return cursor back to its start position before emitting a statement that will evaluate true, or false
                branchCursor.Index = cursorIndex;
                branchCursor.Emit(branchCode, branchTarget);
                return true;
            }

            return false;
        }

        public static ILLabel FindBranchTarget(this ILCursor cursor, MoveType moveType, params Func<Instruction, bool>[] matchPredicates)
        {
            ILLabel branchTarget = null;
            if (cursor.TryGotoNext(moveType, matchPredicates))
            {
                //Define, and mark label for future branching
                branchTarget = cursor.Context.DefineLabel();
                cursor.MarkLabel(branchTarget);
            }

            return branchTarget;
        }
    }
}
