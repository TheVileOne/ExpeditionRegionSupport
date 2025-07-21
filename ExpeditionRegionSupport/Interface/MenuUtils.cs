using Menu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExpeditionRegionSupport.Interface
{
    public static class MenuUtils
    {
        /// <summary>
        /// Changes the size of all SimpleButton objects to the SimpleButton with the longest width
        /// </summary>
        /// <param name="menuObjects">The list of MenuObjects to use</param>
        /// <param name="resizeImmune">Include any buttons that should be ignored here</param>
        public static void UpdateButtonSize<T>(List<T> menuObjects, params SimpleButton[] resizeImmune) where T : MenuObject
        {
            float highestWidth = 0f;
            List<SimpleButton> buttons = new List<SimpleButton>();

            foreach (MenuObject obj in menuObjects)
            {
                SimpleButton button = obj as SimpleButton;

                if (button != null && !resizeImmune.Contains(button))
                {
                    highestWidth = Mathf.Max(highestWidth, button.size.x);
                    buttons.Add(button);
                }
            }

            foreach (SimpleButton button in buttons)
                button.SetSize(new Vector2(highestWidth, button.size.y));
        }

        /// <summary>
        /// Defines the before and after selectables for the first and last MenuObject in a list
        /// </summary>
        /// <param name="menuObjects">The list of MenuObjects to use</param>
        /// <param name="selectableBefore">The selectable before the first object in the list</param>
        /// <param name="selectableAfter">The selectable after the last object in the list</param>
        public static void SetSelectables<T>(List<T> menuObjects, MenuObject selectableBefore, MenuObject selectableAfter = null) where T : MenuObject
        {
            if (selectableAfter == null)
                selectableAfter = selectableBefore;

            MenuObject firstObject = menuObjects.First();
            MenuObject lastObject = menuObjects.Last();

            firstObject.nextSelectable[1] = selectableBefore;
            lastObject.nextSelectable[3] = selectableAfter;

            selectableBefore.nextSelectable[3] = firstObject;
            selectableAfter.nextSelectable[1] = lastObject;
        }
    }
}
