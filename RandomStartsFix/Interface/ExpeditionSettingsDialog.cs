using ExpeditionRegionSupport.Settings;
using Extensions;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ModManager;
using Vector2 = UnityEngine.Vector2;

namespace ExpeditionRegionSupport.Interface
{
    public class ExpeditionSettingsDialog : FilterDialog
    {
        public FilterDialogExtension.FilterDialogCWT CWT => this.GetCWT();
        public ScrollablePage MainPage
        {
            get => CWT.Page;
            set => CWT.Page = value;
        }

        #region InitValues

        /// <summary>
        /// The common distance between two vertically adjacent selectables 
        /// </summary>
        public const float VERTICAL_PADDING = 10f;

        /// <summary>
        /// The height of a SimpleButton in this dialog
        /// </summary>
        public const float BUTTON_HEIGHT = 35f;

        public readonly string HeaderText = "SETTINGS";
        public readonly string SubHeaderText = "Region filter";

        #endregion

        private CheckBox shelterDetectionCheckBox;
        private SimpleButton restoreDefaultButton;
        private SimpleButton reloadFromFileButton;
        private SimpleButton customizeSpawnsButton;

        private FilterCheckBox regionFilterVanilla;
        private FilterCheckBox regionFilterMoreSlugcats;
        private FilterCheckBox regionFilterCustom;
        private FilterCheckBox regionFilterVisitedOnly;

        //TODO:
        //Show custom regions available in Expedition?

        public ExpeditionSettingsDialog(ProcessManager manager, ChallengeSelectPage owner) : base(manager, owner)
        {
            heading.text = "REGION FILTER";
            description.text = "Filter spawnable region categories when randomising";

            InitializeButtons();
        }

        public void ReloadFiles()
        {
            ModMerger modMerger = new ModMerger();

            //TODO
        }

        public void InitializeButtons()
        {
            PositionSpacer spacer = new PositionSpacer(new Vector2(683f, 265f), BUTTON_HEIGHT, VERTICAL_PADDING);

            bool firstHandled = false;
            MainPage.AddSubObject(CreateButton("Reload Expedition Files", ExpeditionSignal.RELOAD_MOD_FILES, spacer, ref firstHandled));
            MainPage.AddSubObject(CreateButton("Restore Defaults", ExpeditionSignal.RESTORE_DEFAULTS, spacer, ref firstHandled));
            MainPage.AddSubObject(CreateButton("Customize Spawns", ExpeditionSignal.OPEN_SPAWN_DIALOG, spacer, ref firstHandled));
        }

        public SimpleButton CreateButton(string buttonTextRaw, string signalText, PositionSpacer spacer, ref bool firstHandled)
        {
            Vector2 buttonPos = firstHandled ? spacer.NextPosition : spacer.CurrentPosition;

            try
            {
                return CreateButton(buttonTextRaw, signalText, buttonPos);
            }
            finally
            {
                firstHandled = true;
            }
        }

        public SimpleButton CreateButton(string buttonTextRaw, string signalText, Vector2 pos)
        {
            string buttonTextTranslated = Translate(buttonTextRaw);

            //Adjust button width to accomodate varying translation lengths
            float buttonWidth = Math.Max(85f, Menu.Remix.MixedUI.LabelTest.GetWidth(buttonTextTranslated, false) + 10f); //+10 is the padding

            //Creates a button aligned vertically in the center of the screen
            SimpleButton button = new SimpleButton(this, MainPage, buttonTextTranslated, signalText,
                         new Vector2(pos.x - (buttonWidth / 2f), pos.y), new Vector2(buttonWidth, BUTTON_HEIGHT)); //pos, size

            //A next selectable doesn't exist for these directions
            button.nextSelectable[0] = button;
            button.nextSelectable[2] = button;

            return button;
        }

        public void InitializeCheckBoxes()
        {
            regionFilterVanilla = CreateCheckBox("Vanilla Regions", ExpeditionSettings.Filters.AllowVanillaRegions, 0, "VANILLA");
            regionFilterMoreSlugcats = CreateCheckBox("More Slugcats Regions", ExpeditionSettings.Filters.AllowMoreSlugcatsRegions, 1, "MORE SLUGCATS");
            regionFilterCustom = CreateCheckBox("Custom Regions", ExpeditionSettings.Filters.AllowCustomRegions, 2, "CUSTOM");
            regionFilterVisitedOnly = CreateCheckBox("Visited Regions Only", ExpeditionSettings.Filters.VisitedRegionsOnly, 3, "VISITED ONLY", true);
        }

        public FilterCheckBox CreateCheckBox(string labelText, SimpleToggle optionState, int checkBoxIndex, string checkBoxIDString, bool isLastCheckBox = false)
        {
            Vector2 defaultLabelPosition = new Vector2(553, 590);
            Vector2 defaultCheckBoxPosition = new Vector2(793, 577);
            //Vector2 defaultDividerPosition = new Vector2(684 - leftAnchor, 571);

            float checkBoxHeight = 37f * checkBoxIndex; //This affects how checkboxes stack 

            Vector2 actualLabelPosition = new Vector2(defaultLabelPosition.x, defaultLabelPosition.y - checkBoxHeight);
            Vector2 actualCheckBoxPosition = new Vector2(defaultCheckBoxPosition.x, defaultCheckBoxPosition.y - checkBoxHeight);
            //Vector2 actualDividerPosition = new Vector2(defaultDividerPosition.x, defaultDividerPosition.y - checkBoxHeight);

            //TODO: Move inside FilterCheckBox???
            MenuLabel checkBoxLabel = new MenuLabel(this, MainPage, labelText, actualLabelPosition, default, true, null);
            checkBoxLabel.label.alignment = FLabelAlignment.Left;

            FilterCheckBox checkBox = new FilterCheckBox(this, CWT.Options, optionState, actualCheckBoxPosition, 0f, string.Empty, checkBoxIDString, false)
            {
                label = checkBoxLabel
            };

            checkBox.AddSubObject(checkBoxLabel);

            CWT.Options.AddOption(checkBox);

            //Handle control navigation
            checkBox.nextSelectable[0] = checkBox;
            checkBox.nextSelectable[2] = checkBox;
            if (checkBoxIndex == 0)
            {
                checkBox.nextSelectable[1] = cancelButton;
                cancelButton.nextSelectable[3] = checkBox;
            }

            if (isLastCheckBox)
            {
                checkBox.nextSelectable[3] = cancelButton;
                cancelButton.nextSelectable[1] = checkBox;
            }
            else if (checkBoxIndex > 0)
            {
                /*
                 FSprite dividerSprite = new FSprite("pixel", true);

                 dividerSprite.SetPosition(actualDividerPosition);
                 dividerSprite.scaleX = 270f;
                 dividerSprite.color = new Color(0.4f, 0.4f, 0.4f);
                 container.AddChild(dividerSprite);
                 dividers.Add(dividerSprite);
                */
            }

            return checkBox;
        }

        public override void Singal(MenuObject sender, string signal)
        {
            switch (signal)
            {
                case ExpeditionSignal.RESTORE_DEFAULTS:
                    {
                        ExpeditionSettings.RestoreToDefaults();
                        return;
                    }
                case ExpeditionSignal.RELOAD_MOD_FILES:
                    {
                        ReloadFiles();
                        return;
                    }
                case ExpeditionSignal.OPEN_SPAWN_DIALOG:
                    {
                        //Not implemented yet
                        return;
                    }
            }

            base.Singal(sender, signal);
        }
    }
}
