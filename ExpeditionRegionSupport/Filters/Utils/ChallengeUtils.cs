using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public static class ChallengeUtils
    {
        /// <summary>
        /// Gets the challenge reference used by ChallengeOrganizer to manage challenge assignment
        /// </summary>
        public static Challenge GetChallengeType(Challenge challenge)
        {
            return ChallengeOrganizer.availableChallengeTypes.FindType(challenge);
        }

        public static string GetTypeName(this Challenge self)
        {
            return self.GetType().Name;
        }
    }
}
