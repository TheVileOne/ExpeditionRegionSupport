﻿using Expedition;
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
        public static readonly Menu.Menu.MenuColors DISABLED_HOVER = new Menu.Menu.MenuColors("DISABLE HOVER", true); 

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
        public static readonly HSLColor DISABLE_COLOR = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkRed);
        public static readonly HSLColor DISABLE_HIGHLIGHT_COLOR = new HSLColor(0.033333335f, 0.65f, 0.4f);
        public static readonly HSLColor HIDDEN_COLOR = new HSLColor(0.12f, 0.8f, 0.55f);

        /// <summary>
        /// The number of aborted slots to display in the slot collection 
        /// </summary>
        public static int AbortedSlotCount;

        /// <summary>
        /// Checks that slot is within the aborted slot range. (This range is always after the list of challenges)
        /// </summary>
        public static bool IsAbortedSlot(int slot)
        {
            return AbortedSlotCount > 0 && slot >= SlotChallenges.Count && slot < SlotChallenges.Count + AbortedSlotCount;
        }

        /// <summary>
        /// Returns the earliest index of a non-hidden slot
        /// </summary>
        public static int FirstPlayableSlot()
        {
            return SlotChallenges.FindIndex(c => !c.hidden);
        }

        public static void UpdateSlotVisuals(int slot)
        {
            BigSimpleButton slotButton = SlotButtons[slot];
            //Challenge slotChallenge = SlotChallenges[slot];

            if (IsAbortedSlot(slot))
            {
                slotButton.inactive = false;
                slotButton.buttonBehav.greyedOut = false;
                slotButton.GetCWT().HighlightColor = DISABLED_HOVER;
                slotButton.labelColor = DISABLE_COLOR;
                slotButton.rectColor = DISABLE_COLOR;
            }
            else if (slotButton.rectColor.HasValue && slotButton.rectColor.Value.Equals(DISABLE_COLOR))
            {
                slotButton.GetCWT().HighlightColor = ExtensionMethods.ButtonTemplateCWT.DEFAULT_HIGHLIGHT_COLOR;
                slotButton.labelColor = DEFAULT_COLOR;
                slotButton.rectColor = DEFAULT_COLOR;
            }

            /*
            if (slotChallenge.GetCWT().Disabled)
            {
                slotButton.inactive = false;
                slotButton.buttonBehav.greyedOut = false;
                slotButton.GetCWT().HighlightColor = DISABLED_HOVER;
                slotButton.labelColor = DISABLE_COLOR;
                slotButton.rectColor = DISABLE_COLOR;
            }
            else if (slotButton.rectColor.HasValue && slotButton.rectColor.Value.Equals(DISABLE_COLOR))
            {
                slotButton.GetCWT().HighlightColor = ExtensionMethods.ButtonTemplateCWT.DEFAULT_HIGHLIGHT_COLOR;
                slotButton.labelColor = slotChallenge.hidden ? HIDDEN_COLOR : DEFAULT_COLOR;
                slotButton.rectColor = DEFAULT_COLOR;
            }
            */
        }

        public static void ClearAbortedSlots()
        {
            AbortedSlotCount = 0;
        }
    }
}
