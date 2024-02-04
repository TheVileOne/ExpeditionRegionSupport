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
using UnityEngine;

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
            cwt.Options = new FilterOptions(self, cwt.Page, cwt.Page.pos);

            cwt.Page.subObjects.Add(cwt.Options);
        }

        public static void PreUpdate(this FilterDialog self)
        {
            var cwt = self.GetCWT();

            //Set pre-existing fields now maintained through the main page. These are kept for compatibility reasons.
            self.currentAlpha = cwt.Page.CurrentAlpha;
            self.targetAlpha = cwt.Page.TargetAlpha;
            self.closing = cwt.Page.Closing;
            self.opening = cwt.Page.Opening;

            if (cwt.RunOnNextUpdate != null)
            {
                cwt.RunOnNextUpdate.Invoke(self);
                cwt.RunOnNextUpdate = null;
            }

            Plugin.Logger.LogInfo(self.currentAlpha);
            Plugin.Logger.LogInfo(self.targetAlpha);
            Plugin.Logger.LogInfo(self.closing);
            Plugin.Logger.LogInfo(self.opening);
        }

        public static void CloseFilterDialog(this FilterDialog self)
        {
            self.pageTitle.RemoveFromContainer();
            self.manager.StopSideProcess(self);
        }

        //MenuObject

        /// <summary>
        /// Changes both position and last position to a given Vector2
        /// </summary>
        public static void SetPosition(this PositionedMenuObject self, Vector2 pos)
        {
            self.pos = self.lastPos = pos;
        }

        /// <summary>
        /// Changes the x-value of both position and last position
        /// </summary>
        public static void SetPosX(this PositionedMenuObject self, float x)
        {
            self.pos.x = self.lastPos.x = x;
        }

        /// <summary>
        /// Changes the y-value of both position and last position
        /// </summary>
        public static void SetPosY(this PositionedMenuObject self, float y)
        {
            self.pos.y = self.lastPos.y = y;
        }

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

        //MenuLabel

        public static void SetColor(this MenuLabel self, Color color)
        {
            self.label.color = color;
        }

        //FContainer

        /// <summary>
        /// Returns all children belonging to this container in a list
        /// </summary>
        public static List<FNode> GetAllChildren(this FContainer parent)
        {
            return new List<FNode>(parent._childNodes);
        }

        /// <summary>
        /// Take all child FNodes in one container and place them in another container
        /// </summary>
        public static void MoveChildrenToNewContainer(this FContainer currentParent, FContainer newParent)
        {
            List<FNode> newChildList = currentParent.GetAllChildren();

            currentParent.RemoveAllChildren();
            newChildList.ForEach(newParent.AddChild);
        }
    }
}
