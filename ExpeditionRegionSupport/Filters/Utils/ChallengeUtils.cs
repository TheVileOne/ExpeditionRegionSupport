using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public static class ChallengeUtils
    {
        public class ChallengeCWT
        {
            /// <summary>
            /// A flag that indicates that this challenge cannot be chosen for an active mission
            /// </summary>
            public bool Disabled = false;
        }

        public static readonly ConditionalWeakTable<Challenge, ChallengeCWT> challengeCWT = new();

        public static ChallengeCWT GetCWT(this Challenge self) => challengeCWT.GetValue(self, _ => new());

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
