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

        public static SlotInfo Info = new SlotInfo();

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
            return GetSlotStatus(slot) == SlotStatus.Unavailable;
        }

        public static SlotStatus GetSlotStatus(int slot)
        {
            SlotStatus status;
            if (slot < SlotChallenges.Count)
                status = SlotStatus.Filled;
            else if (AbortedSlotCount > 0 && slot < SlotChallenges.Count + AbortedSlotCount)
                status = SlotStatus.Unavailable;
            else
                status = SlotStatus.Empty;
            return status;
        }

        /// <summary>
        /// Returns the earliest index of a non-hidden slot
        /// </summary>
        public static int FirstPlayableSlot()
        {
            return SlotChallenges.FindIndex(c => !c.hidden);
        }

        /// <summary>
        /// Handles any logic that needs to run when slot changes are processed
        /// </summary>
        public static void UpdateSlot(int slot)
        {
            BigSimpleButton slotButton = SlotButtons[slot];

            SlotStatus status = GetSlotStatus(slot);

            switch (status)
            {
                case SlotStatus.Filled:
                    Info.SlotCount.Challenges++;
                    break;
                case SlotStatus.Empty:
                    Info.SlotCount.Empty++;
                    break;
                case SlotStatus.Unavailable:
                    Info.SlotCount.Unavailable++;
                    break;
            }

            if (status == SlotStatus.Unavailable)
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
                    if (firstAbortedSlot < SlotChallenges.Count) //Make sure that slot range is involves active challenges before removal
                    {
                        int challengeRemoveCount = SlotChallenges.Count - firstAbortedSlot;

                        SlotChallenges.RemoveRange(firstAbortedSlot, challengeRemoveCount);
                    }
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

        public class SlotInfo
        {
            public SlotCountInfo SlotCount = new SlotCountInfo();
            public SlotCountInfo LastSlotCount = new SlotCountInfo();
            public SlotChangeInfo SlotChanges = new SlotChangeInfo();

            public void NotifyChange(int slot, SlotChange changeType)
            {
                switch (changeType)
                {
                    case SlotChange.Add:
                        SlotChanges.Added.Add(slot);
                        break;
                    case SlotChange.HiddenReveal:
                        SlotChanges.HiddenReveal.Add(slot);
                        break;
                    case SlotChange.Remove:
                        SlotChanges.Removed.Add(slot);
                        break;
                    case SlotChange.Replace:
                        SlotChanges.Replaced.Add(slot);
                        break;
                }
            }

            /// <summary>
            /// Prepares object to receive new process information
            /// </summary>
            public void NewProcess()
            {
                LastSlotCount.Challenges = SlotCount.Challenges;
                LastSlotCount.Empty = SlotCount.Empty;
                LastSlotCount.Unavailable = SlotCount.Unavailable;

                SlotCount.Challenges = SlotCount.Empty = SlotCount.Unavailable = 0;

                SlotChanges.Added.Clear();
                SlotChanges.HiddenReveal.Clear();
                SlotChanges.Removed.Clear();
                SlotChanges.Replaced.Clear();
            }

            public void AnalyzeChanges()
            {
                analyzeCount("CHALLENGE SLOTS", SlotCount.Challenges - LastSlotCount.Challenges);
                analyzeCount("UNAVAILABLE SLOTS", SlotCount.Unavailable - LastSlotCount.Unavailable);
                analyzeCount("EMPTY SLOTS", SlotCount.Empty - LastSlotCount.Empty);

                void analyzeCount(string header, int countDelta)
                {
                    Plugin.Logger.LogInfo(header);

                    if (countDelta == 0)
                        Plugin.Logger.LogInfo("UNCHANGED");
                    else if (countDelta > 0)
                        Plugin.Logger.LogInfo($"INCREASED BY {countDelta}");
                    else
                        Plugin.Logger.LogInfo($"DECREASED BY {Math.Abs(countDelta)}");
                }
            }

            public class SlotCountInfo
            {
                public int Empty;
                public int Challenges; //Hidden + Playable
                public int Unavailable;
            }

            public class SlotChangeInfo
            {
                public List<int> Added = new List<int>();
                public List<int> Removed = new List<int>();
                public List<int> Replaced = new List<int>();
                public List<int> HiddenReveal = new List<int>();
            }
        }
    }

    public enum SlotStatus
    {
        Empty,
        Filled,
        Unavailable
    }

    public enum SlotChange
    {
        Add,
        HiddenReveal,
        Remove,
        Replace
    }
}
