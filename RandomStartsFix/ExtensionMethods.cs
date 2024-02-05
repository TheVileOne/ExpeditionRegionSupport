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

            /// <summary>
            /// Indicates that the constructor has successfully run without an exception
            /// </summary>
            public bool InitSuccess;

            /// <summary>
            /// Ensures that the close dialog trigger is handled only once
            /// </summary>
            public bool PauseButtonHandled;
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

        /// <summary>
        /// Sets field values back to base values
        /// </summary>
        public static void SetToDefault(this FilterDialog self)
        {
            ScrollablePage page = self.GetCWT().Page;

            page.SetToDefault();
            self.AssignValuesFromPage();
        }

        public static void AssignValuesFromPage(this FilterDialog self)
        {
            ScrollablePage page = self.GetCWT().Page;

            self.uAlpha = page.BaseAlpha;
            self.currentAlpha = page.Alpha;
            self.lastAlpha = page.LastAlpha;
            self.targetAlpha = page.TargetAlpha;

            self.opening = page.Opening;
            self.closing = page.Closing;
        }

        public static void CloseFilterDialog(this FilterDialog self, bool instantClose = false)
        {
            //Normally there is a fade out process before close. This code will bypass it.
            if (instantClose)
            {
                self.SetToDefault();

                self.owner.unlocksButton.greyedOut = false;
                self.owner.startButton.greyedOut = false;

                self.pageTitle.RemoveFromContainer();
                self.manager.StopSideProcess(self);
                return;
            }

            self.Singal(null, "CLOSE");
        }

        public static void LogValues(this FilterDialog self)
        {
            if (self.closing || self.opening)
            {
                Plugin.Logger.LogInfo(self.closing ? "Closing" : "Opening");
                Plugin.Logger.LogInfo("Current Alpha: " + self.currentAlpha);
                Plugin.Logger.LogInfo("Target Alpha: " + self.targetAlpha);
            }
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
