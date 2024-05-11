using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpeditionRegionSupport
{
    public static class SlugcatUtils
    {
        /// <summary>
        /// Represents a group inclusive SlugcatStats.Name instance 
        /// </summary>
        public static SlugcatStats.Name AnySlugcat = new SlugcatStats.Name("ANY");

        public static readonly SlugcatStats.Name[] VanillaSlugcats =
        {
            SlugcatStats.Name.Yellow,
            SlugcatStats.Name.White,
            SlugcatStats.Name.Red
        };

        /// <summary>
        /// Indicates that all vanilla, and MSC slugcats have had their ExtEnum field initialized
        /// </summary>
        public static bool SlugcatsInitialized;

        /// <summary>
        /// For useful purposes, Downpour slugcats are not considered modded
        /// </summary>
        public static bool IsModcat(SlugcatStats.Name slugcat)
        {
            return !IsVanillaSlugcat(slugcat) && !SlugcatStats.IsSlugcatFromMSC(slugcat)
                && !slugcat.Equals(SlugcatStats.Name.Night) && !slugcat.Equals(MoreSlugcatsEnums.SlugcatStatsName.Slugpup);
        }

        public static bool IsVanillaSlugcat(SlugcatStats.Name slugcat)
        {
            return VanillaSlugcats.Contains(slugcat);
        }

        /// <summary>
        /// Gets the SlugcatStats.Name associated with a name string, or creates one if not registered
        /// </summary>
        /// <param name="name">The slugcat name (ExtEnum value field)</param>
        public static SlugcatStats.Name GetOrCreate(string name)
        {
            if (TryParse(name, out SlugcatStats.Name found))
                return found;

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

        public static void LogAllSlugcats()
        {
            Plugin.Logger.LogInfo("Registered slugcats");
            foreach (string slugcat in ExtEnumBase.GetNames(typeof(SlugcatStats.Name)))
                Plugin.Logger.LogInfo(slugcat);
        }
    }
}
