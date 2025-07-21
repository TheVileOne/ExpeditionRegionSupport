﻿using Expedition;
using Extensions;
using Menu;
using System;
using System.Collections.Generic;

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
        /// This shouldn't be necessary.
        /// TODO: Figure out why counts are going out of sync
        /// </summary>
        public static void RefreshSlotCounts()
        {
            Info.SlotCount.Challenges = 0;
            Info.SlotCount.Empty = 0;
            Info.SlotCount.Unavailable = 0;

            foreach (var slot in SlotButtons)
            {
                if (slot.menuLabel.text == "EMPTY")
                    Info.SlotCount.Empty++;
                else if (slot.menuLabel.text == "UNAVAILABLE")
                    Info.SlotCount.Unavailable++;
                else
                    Info.SlotCount.Challenges++;
            }

            AbortedSlotCount = Info.SlotCount.Unavailable;
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

            var cwt = slotButton.GetCWT();

            cwt.IsChallengeSlot = true;

            if (status == SlotStatus.Unavailable)
            {
                slotButton.inactive = true; //Prevents selection
                slotButton.buttonBehav.greyedOut = false;

                //Update hover, and display colors
                cwt.HighlightColor = DISABLED_HOVER;
                slotButton.labelColor = DISABLE_COLOR;
                slotButton.rectColor = DISABLE_COLOR;
            }
            else if (slotButton.rectColor.HasValue && slotButton.rectColor.Value.Equals(DISABLE_COLOR))
            {
                slotButton.inactive = false;

                //Update hover, and display colors
                cwt.HighlightColor = ExtensionMethods.ButtonTemplateCWT.DEFAULT_HIGHLIGHT_COLOR;
                slotButton.labelColor = DEFAULT_COLOR;
                slotButton.rectColor = DEFAULT_COLOR;
            }
        }

        /// <summary>
        /// Adjusts the value of AbortedSlotCount by a positive, or negative amount
        /// </summary>
        public static void AdjustAbortedSlots(int countDelta)
        {
            AbortedSlotCount = RWCustom.Custom.IntClamp(AbortedSlotCount + countDelta, 0, SlotChallenges.Count - 1);
        }

        public static void ClearAbortedSlots()
        {
            AbortedSlotCount = 0;
        }

        /// <summary>
        /// Maintains the number of unavilable slots to show to the player
        /// </summary>
        public static void UpdateAbortedSlots()
        {
            if (ChallengeAssignment.Aborted)
            {
                if (ChallengeAssignment.ChallengesRequested <= 1)
                {
                    //Hacky solution: Prevents toggling hidden, or rerolling specific slots from affecting unavailable slots.
                    //For some reason these actions are sometimes creating garbage slot info such as reporting an add event on abort.
                    //This solution restores last counts, and clears Add/Remove events, but keeps replace event reports intact, and it seems to work.
                    Info.Restore();
                    return; //Ignore like the request never happened
                }

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
                    AbortedSlotCount = ChallengeAssignment.RequestsRemaining;

                    //Remove challenges that are no longer available for the current Expedition
                    if (firstAbortedSlot < SlotChallenges.Count) //Make sure that slot range is involves active challenges before removal
                    {
                        int challengeRemoveCount = SlotChallenges.Count - firstAbortedSlot;

                        SlotChallenges.RemoveRange(firstAbortedSlot, challengeRemoveCount);

                        for (int i = 0; i < challengeRemoveCount; i++)
                            Info.NotifyChange(firstAbortedSlot + i, SlotChange.Remove);
                    }
                }
                else //The behavior when there is no valid playable slots. Hidden slots are not considered playable at the start of an Expedition.
                {
                    //Ultimately, the abort process will be ignored under this condition, and any successful requests handled will be kept along with
                    //the prexisting challenges that couldn't be replaced. Unavailable slots from previous requests will be unaffected.
                    Plugin.Logger.LogInfo("Request could not be handled due to current filter conditions. No playable slot was available.");
                }
            }
            else if (AbortedSlotCount > 0)
            {
                //The number of challenge slots added, or removed since the last slot button update 
                int slotCountDelta = 0;
                if (Info.SlotChanges.Added.Count > 0)
                    slotCountDelta = Info.SlotChanges.Added.Count;
                else if (Info.SlotChanges.Removed.Count > 0)
                    slotCountDelta = Info.SlotChanges.Removed.Count * -1;

                //Plugin.Logger.LogDebug("DELTA: " + slotCountDelta);
                if (slotCountDelta > 0) //Behavior for plus button
                {
                    AdjustAbortedSlots(slotCountDelta * -1); //There is an inverse relation between the overall slot change delta and the aborted one
                }
                else if (slotCountDelta < 0) //Behavior for minus button. Do not leave an open gap.
                {
                    //AdjustAbortedSlots(Math.Abs(slotCountDelta));
                    ClearAbortedSlots();
                }
                //A delta of zero means that a change was made that did not impact the number of available slots.
                //These actions should not affect the unavailable slot count.
            }

            //Plugin.Logger.LogDebug("ABORTED COUNT: " + AbortedSlotCount);
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

            public void Restore()
            {
                SlotCount.Challenges = LastSlotCount.Challenges;
                SlotCount.Empty = LastSlotCount.Empty;
                SlotCount.Unavailable = LastSlotCount.Unavailable;

                SlotChanges.Added.Clear();
                SlotChanges.Removed.Clear();
            }

            public void AnalyzeChanges()
            {
                bool firstProcess = true;
                analyzeCount("CHALLENGE SLOTS", SlotCount.Challenges - LastSlotCount.Challenges);
                analyzeCount("UNAVAILABLE SLOTS", SlotCount.Unavailable - LastSlotCount.Unavailable);
                analyzeCount("EMPTY SLOTS", SlotCount.Empty - LastSlotCount.Empty);

                Plugin.Logger.LogInfo("Challenge Slots: " + SlotCount.Challenges);
                Plugin.Logger.LogInfo("Unavailable Slots: " + SlotCount.Unavailable);
                Plugin.Logger.LogInfo("Empty Slots: " + SlotCount.Empty);

                if (SlotChanges.Added.Count > 0)
                    logInfo("Added", SlotChanges.Added.Count);

                if (SlotChanges.Removed.Count > 0)
                    logInfo("Removed", SlotChanges.Removed.Count);

                if (SlotChanges.HiddenReveal.Count > 0)
                    logInfo("Revealed", SlotChanges.HiddenReveal.Count);

                if (SlotChanges.Replaced.Count > 0)
                    logInfo("Replaced", SlotChanges.Replaced.Count);

                void analyzeCount(string header, int countDelta)
                {
                    if (firstProcess)
                    {
                        Plugin.Logger.LogInfo("--------------");
                        firstProcess = false;
                    }
                    Plugin.Logger.LogInfo(header);

                    if (countDelta == 0)
                        Plugin.Logger.LogInfo("UNCHANGED");
                    else if (countDelta > 0)
                        Plugin.Logger.LogInfo($"INCREASED BY {countDelta}");
                    else
                        Plugin.Logger.LogInfo($"DECREASED BY {Math.Abs(countDelta)}");
                    Plugin.Logger.LogInfo("--------------");
                }

                void logInfo(string dataID, int dataCount)
                {
                    Plugin.Logger.LogInfo($"Challenges {dataID}: {dataCount}");
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
