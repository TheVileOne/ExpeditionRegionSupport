using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpeditionRegionSupport
{
    public static class SlugcatUtils
    {
        /// <summary>
        /// Gets the SlugcatStats.Name associated with a name string, or creates one if not registered
        /// </summary>
        /// <param name="name">The slugcat name (ExtEnum value field)</param>
        public static SlugcatStats.Name GetOrCreate(string name)
        {
            if (TryParse(name, out SlugcatStats.Name found))
                return found;

            Plugin.Logger.LogInfo("Unrecognized slugcat processed");
            return new SlugcatStats.Name(name.Trim());
        }

        public static bool TryParse(string name, out SlugcatStats.Name found)
        {
            name = NameFromAlias(name.Trim());

            if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), name, true, out ExtEnumBase extBase))
            {
                found = (SlugcatStats.Name)extBase;
                return true;
            }

            found = default;
            return false;
        }

        /// <summary>
        /// Takes a slugcat name alias and returns the name used by SlugcatStats.Name
        /// </summary>
        public static string NameFromAlias(string name)
        {
            if (name.Equals(@"Monk", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Yellow";
            }
            else if (name.Equals(@"Survivor", StringComparison.InvariantCultureIgnoreCase))
            {
                return "White";
            }
            else if (name.Equals(@"Hunter", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Red";
            }
            else if (name.Equals(@"Spearmaster", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Spear";
            }
            else if (name.Equals(@"Sophaniel", StringComparison.InvariantCultureIgnoreCase)
                  || name.Equals(@"Sophanthiel", StringComparison.InvariantCultureIgnoreCase)
                  || name.Equals(@"Sofaniel", StringComparison.InvariantCultureIgnoreCase)) //Inv is a valid name 
            {
                return "Sofanthiel";
            }

            return name;
        }
    }
}
