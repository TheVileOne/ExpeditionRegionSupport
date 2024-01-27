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

        public static void InitializePage(this FilterDialog self)
        {
            var cwt = self.GetCWT();

            cwt.Page = new ScrollablePage(self, null, "main", 0);
            cwt.Options = new FilterOptions(self, cwt.Page, new UnityEngine.Vector2(0, 0)); //Default pos is placeholder

            cwt.Page.subObjects.Add(cwt.Options);
        }

        //MenuObject

        public static void SetAlpha(this MenuObject self, float alpha)
        {
            self.Container.alpha = alpha;

            foreach (MenuObject child in self.subObjects)
                child.SetAlpha(alpha);
        }

        public static void ChangeOwner(this MenuObject self, MenuObject newOwner)
        {
            if (self.owner != null)
                self.owner.RemoveSubObject(self);

            self.owner = newOwner;
            self.ChangeMenu(newOwner?.menu);

            newOwner.subObjects.Add(self);
        }

        public static void ChangeMenu(this MenuObject self, Menu.Menu newMenu)
        {
            self.menu = newMenu;
            self.subObjects.ForEach(child => child.ChangeMenu(newMenu));
        }

        public static void AddSubObject(this MenuObject self, MenuObject child)
        {
            if (child == null)
            {
                Plugin.Logger.LogWarning("Attempted to add a null MenuObject to " + self);
                return;
            }

            if (child.owner != self) //Probably assigned through constructor
                child.ChangeOwner(self);

            self.subObjects.Add(child);
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
