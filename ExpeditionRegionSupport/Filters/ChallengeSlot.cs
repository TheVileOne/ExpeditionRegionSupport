using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using Extensions;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters
{
    /// <summary>
    /// A class for handling challenge slots, and accessing Expedition fields related to them
    /// </summary>
    public static class ChallengeSlot
    {
        /// <summary>
        /// A fixed array of buttons that Expedition uses to store mission challenges. These are stored in a ChallengeSelectPage instance.
        /// This field gets updated each time UpdateChallengeButtons is called.
        /// </summary>
        public static BigSimpleButton[] SlotButtons;

        /// <summary>
        /// The list of challenges in the order they would be displayed in Challenge Select screen for a given selected slugcat
        /// </summary>
        public static List<Challenge> SlotChallenges => ExpeditionData.challengeList;

        public static readonly HSLColor DEFAULT_COLOR = new HSLColor(0f, 0f, 0.7f);
        public static readonly HSLColor DISABLE_COLOR = new HSLColor(0.8f, 5f, 0.7f);
        public static readonly HSLColor HIDDEN_COLOR = new HSLColor(0.12f, 0.8f, 0.55f);

        public static void UpdateSlotVisuals(int slot)
        {
            BigSimpleButton slotButton = SlotButtons[slot];
            Challenge slotChallenge = SlotChallenges[slot];

            if (slotChallenge.GetCWT().Disabled)
            {
                slotButton.inactive = false;
                slotButton.buttonBehav.greyedOut = false;
                slotButton.labelColor = DISABLE_COLOR;
                slotButton.rectColor = DISABLE_COLOR;
            }
            else if (slotButton.rectColor.HasValue && slotButton.rectColor.Value.Equals(DISABLE_COLOR))
            {
                slotButton.labelColor = slotChallenge.hidden ? HIDDEN_COLOR : DEFAULT_COLOR;
                slotButton.rectColor = DEFAULT_COLOR;
            }
        }
    }
}
