using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using Menu;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Interface.Components
{
    public class ChallengeSlotCollection
    {
        /// <summary>
        /// The number of slot buttons per challenge page
        /// </summary>
        public const int SLOTS_PER_PAGE = 6;

        public List<Challenge> Challenges;
        public BigSimpleButton[] SlotButtons;

        public ChallengeSelectPage Owner;

        public ChallengeSlotCollection(ChallengeSelectPage collectionOwner, SlugcatStats.Name challengeOwner)
        {
            Owner = collectionOwner;
            UpdateChallenges(challengeOwner);
        }

        public void AddSlot()
        {
        }

        public void RemoveSlot()
        {
        }

        public void GenerateSlots(int slotsWanted)
        {
        }

        public void RerollChallenges()
        {
        }

        public void ToggleHidden(int slotID)
        {
        }

        public void UpdateChallenges(SlugcatStats.Name challengeOwner)
        {
            Challenges = ChallengeUtils.GetChallenges(challengeOwner);
            UpdateChallengeButtons();
        }

        public void UpdateChallengeButtons()
        {
            Owner.UpdateChallengeButtons();
        }
    }
}
