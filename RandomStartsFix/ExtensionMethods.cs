using ExpeditionRegionSupport;
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

namespace Extensions
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
    }
}
