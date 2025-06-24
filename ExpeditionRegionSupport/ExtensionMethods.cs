using ExpeditionRegionSupport;
using Menu;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Extensions
{
    public static class ExtensionMethods
    {
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
            self.owner?.RemoveSubObject(self);

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

        public static void InitializeSelectionNav(this MenuObject self)
        {
            //A next selectable doesn't exist for these directions
            self.nextSelectable[0] = self;
            self.nextSelectable[2] = self;
        }

        //MenuLabel

        public static void SetColor(this MenuLabel self, Color color)
        {
            self.label.color = color;
        }

        //Buttons

        public class ButtonTemplateCWT
        {
            public static readonly Menu.Menu.MenuColors DEFAULT_HIGHLIGHT_COLOR = Menu.Menu.MenuColors.White;

            /// <summary>
            /// The color of a button when hovered, or selected
            /// </summary>
            public Menu.Menu.MenuColors HighlightColor = DEFAULT_HIGHLIGHT_COLOR;

            public bool IsChallengeSlot;
        }

        public static readonly ConditionalWeakTable<ButtonTemplate, ButtonTemplateCWT> buttonTemplateCWT = new();

        public static ButtonTemplateCWT GetCWT(this ButtonTemplate self) => buttonTemplateCWT.GetValue(self, _ => new());

        public class SimpleButtonCWT
        {
            /// <summary>
            /// A flag that indicates that this button must remain in the middle of its parent container / owner
            /// </summary>
            public bool CenterInParent;
        }

        public static readonly ConditionalWeakTable<SimpleButton, SimpleButtonCWT> simpleButtonCWT = new();

        public static SimpleButtonCWT GetCWT(this SimpleButton self) => simpleButtonCWT.GetValue(self, _ => new());

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

        public static readonly ConditionalWeakTable<World, WorldCWT> worldCWT = new();

        public static WorldCWT GetCWT(this World self) => worldCWT.GetValue(self, _ => new());

        public class WorldCWT
        {
            public bool LoadedFromGateTransition;

            /// <summary>
            /// Set by the gate transition hooks to identify the actual gate room code to use when loading into new region
            /// </summary>
            public string LoadRoomTarget;

            /// <summary>
            /// Set by the gate transition hooks to identify the gate room name to check for, and replace when loading into a new region
            /// </summary>
            public string LoadRoomTargetExpected;
        }
    }
}
