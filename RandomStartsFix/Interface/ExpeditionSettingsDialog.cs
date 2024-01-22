using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModManager;
using Vector2 = UnityEngine.Vector2;

namespace ExpeditionRegionSupport.Interface
{
    public class ExpeditionSettingsDialog : FilterDialog
    {
        private CheckBox shelterDetectionCheckBox;
        private SimpleButton reloadButton;

        //private RegionFilter regionFilter; //TODO: Create?

        private CheckboxCollection filterOptions;
        private CheckBox regionFilterVanilla;
        private CheckBox regionFilterMoreSlugcats;
        private CheckBox regionFilterCustom;
        private CheckBox regionFilterVisitedOnly;

        //TODO:
        //Show custom regions available in Expedition?

        public ExpeditionSettingsDialog(ProcessManager manager, ChallengeSelectPage owner) : base(manager, owner)
        {
            float num = 500;
            float globalOffX = 200;//(num - 250f) / -2f;

            //RoundedRect roundedRect = new RoundedRect(this, pages[0], new Vector2(243f + globalOffX, 100f), new Vector2(num, 550f), true);
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "CLOSE")
            {
                PlaySound(SoundID.MENU_Switch_Page_Out);
                manager.StopSideProcess(this);
                return;
            }

            base.Singal(sender, message);
        }

        private bool pauseButtonHandled;
        public override void Update()
        {
            if (!pauseButtonHandled && RWInput.CheckPauseButton(0, manager.rainWorld))
            {
                Singal(null, "EXIT");
                pauseButtonHandled = true;
            }

            base.Update();
        }

        public void ReloadFiles()
        {
            ModMerger modMerger = new ModMerger();

            //TODO
        }


    }
}
