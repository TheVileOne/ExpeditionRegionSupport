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
        public static readonly Menu.Menu.MenuColors DISABLED_HOVER = new Menu.Menu.MenuColors("DISABLE HOVER", true);

        public static int MaxSlotsAllowedCustom = -1;

        public static int MaxSlotsAllowed => MaxSlotsAllowedCustom == -1 ? 5 : MaxSlotsAllowedCustom;

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

        public static void AdjustAbortedSlots(int replaceAmt)
        {
            AbortedSlotCount = RWCustom.Custom.IntClamp(AbortedSlotCount + replaceAmt, 0, SlotChallenges.Count - 1);
        }

        public static void ClearAbortedSlots()
        {
            AbortedSlotCount = 0;
        }

        /// <summary>
        /// Maintains the number of unavilable slots to show to the player
        /// </summary>
        public static void UpdateAbortedSlots(int slotCountDelta)
        {
            if (ChallengeAssignment.Aborted)
            {
                if (ChallengeAssignment.ChallengesRequested <= 1) return; //Ignore like the request never happened

                if (!ChallengeAssignment.SlotsInOrder) //Unusual situation - vanilla Expedition doesn't make non-consecuative batched challenge requests
                {
                    ClearAbortedSlots(); //Since this mod doesn't currently support aborted slot handling, clear the slot count is fine
                    return;
                }

                //This logic typically gets run when the random option is called, and not all requests were handled successfully
                int firstPlayableSlot = FirstPlayableSlot();
                int firstAbortedSlot = ChallengeAssignment.CurrentRequest.Slot;

                if (firstPlayableSlot >= 0 && firstAbortedSlot > firstPlayableSlot) //Ensure there is at least one playable slot
                {
                    for (int slotIndex = firstAbortedSlot; slotIndex < SlotChallenges.Count; slotIndex++)
                    {
                        Challenge challenge = SlotChallenges[slotIndex];

                        //challenge.GetCWT().Disabled = true;
                        challenge.hidden = false; //Hidden doesn't apply to disabled challenges
                    }

                    AbortedSlotCount = ChallengeAssignment.RequestsRemaining;

                    //Remove challenges that are no longer available for the current Expedition
                    SlotChallenges.RemoveRange(SlotChallenges.Count - AbortedSlotCount, AbortedSlotCount);
                }
                else //The behavior when there is no valid playable slots. Hidden slots are not considered playable at the start of an Expedition.
                {
                    //Ultimately, the abort process will be ignored under this condition, and any successful requests handled will be kept along with
                    //the prexisting challenges that couldn't be replaced. Unavailable slots from previous requests will be unaffected.
                    Plugin.Logger.LogInfo("Request could not be handled due to current filter conditions. No playable slot was available.");
                }
            }
            else if (AbortedSlotCount > 0) //Nothing to worry about if there is no aborted slots!
            {
                if (slotCountDelta > 0) //Behavior for plus button
                {
                    AdjustAbortedSlots(slotCountDelta);
                }
                else if (slotCountDelta < 0) //Behavior for minus button. Do not leave an open gap. Do not add more unavailable slots to fill gap.
                {
                    ClearAbortedSlots();
                }
                //A delta of zero means that a change was made that did not impact the number of available slots.
                //These actions should not affect the unavailable slot count.
            }

            if (AbortedSlotCount > 0)
            {
                Plugin.Logger.LogInfo("PLAYABLE SLOTS " + (SlotChallenges.Count - AbortedSlotCount));
                Plugin.Logger.LogInfo("FROZEN SLOTS " + AbortedSlotCount);
            }
        }
    }
}
