using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Interface
{
    public static class MenuUtils
    {
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
