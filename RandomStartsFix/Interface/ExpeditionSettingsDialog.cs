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

        /// <summary>
        /// The height of a CheckBox in this dialog
        /// </summary>
        public const float CHECKBOX_HEIGHT = 35f;

        public readonly string HeaderText = "SETTINGS";
        public readonly string SubHeaderText = "Region filter";

        #endregion

        private SimpleButton cancelChangesButton;
        private SimpleButton reloadFromFileButton;
        private SimpleButton customizeSpawnsButton;

        private FilterCheckBox regionFilterVanilla;
        private FilterCheckBox regionFilterMoreSlugcats;
        private FilterCheckBox regionFilterCustom;
        private FilterCheckBox regionFilterVisitedOnly;

        private FilterCheckBox shelterDetectionCheckBox;

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
            MainPage.AddSubObject(reloadFromFileButton = CreateButton("Reload Expedition Files", ExpeditionSignal.RELOAD_MOD_FILES, spacer, ref firstHandled));
            MainPage.AddSubObject(cancelChangesButton = CreateButton("Restore Defaults", ExpeditionSignal.RESTORE_DEFAULTS, spacer, ref firstHandled));
            MainPage.AddSubObject(customizeSpawnsButton = CreateButton("Customize Spawns", ExpeditionSignal.OPEN_SPAWN_DIALOG, spacer, ref firstHandled));

            UpdateButtonSize();
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

            button.GetCWT().CenterInParent = true; //Keeps button position correct on resize

            return button;
        }

        public void UpdateButtonSize()
        {
            float highestWidth = 0f;
            List<SimpleButton> buttons = new List<SimpleButton>();

            foreach (MenuObject obj in MainPage.subObjects)
            {
                SimpleButton button = obj as SimpleButton;

                if (button != null && button != cancelButton)
                {
                    highestWidth = Mathf.Max(highestWidth, button.size.x);
                    buttons.Add(button);
                }
            }

            foreach (SimpleButton button in buttons)
                button.SetSize(new Vector2(highestWidth, button.size.y));
        }

        public void InitializeCheckBoxes()
        {
            FilterOptions options = CWT.Options;

            PositionSpacer spacer = new PositionSpacer(new Vector2(793f, 577f), CHECKBOX_HEIGHT, 2f);

            bool firstHandled = false;
            options.AddOption(regionFilterVanilla = CreateCheckBox("Vanilla Regions", ExpeditionSettings.Filters.AllowVanillaRegions, "VANILLA", spacer, ref firstHandled));
            options.AddOption(regionFilterMoreSlugcats = CreateCheckBox("More Slugcats Regions", ExpeditionSettings.Filters.AllowMoreSlugcatsRegions, "MORE SLUGCATS", spacer, ref firstHandled));
            options.AddOption(regionFilterCustom = CreateCheckBox("Custom Regions", ExpeditionSettings.Filters.AllowCustomRegions, "CUSTOM", spacer, ref firstHandled));
            options.AddOption(regionFilterVisitedOnly = CreateCheckBox("Visited Regions Only", ExpeditionSettings.Filters.VisitedRegionsOnly, "VISITED ONLY", spacer, ref firstHandled));

            Vector2 nextPos = spacer.NextPosition - new Vector2(0, 80f);

            options.AddOption(shelterDetectionCheckBox = CreateCheckBox("Enable Shelter Detection", ExpeditionSettings.DetectShelterSpawns, "SHELTER_DETECTION", nextPos));

            regionFilterVisitedOnly.FilterImmune = true;
            shelterDetectionCheckBox.FilterImmune = true;

            FilterCheckBox firstOption = options.Boxes.First();
            FilterCheckBox lastOption = options.Boxes.Last();

            //Update keyboard navigation out of FilterOptions
            firstOption.nextSelectable[1] = cancelButton;
            lastOption.nextSelectable[3] = cancelButton;

            cancelButton.nextSelectable[3] = firstOption;
            cancelButton.nextSelectable[1] = lastOption;
        }

        public FilterCheckBox CreateCheckBox(string labelText, SimpleToggle optionState, string IDString, PositionSpacer spacer, ref bool firstHandled)
        {
            Vector2 checkBoxPos = firstHandled ? spacer.NextPosition : spacer.CurrentPosition;

            try
            {
                return CreateCheckBox(labelText, optionState, IDString, checkBoxPos);
            }
            finally
            {
                firstHandled = true;
            }
        }

        public FilterCheckBox CreateCheckBox(string labelText, SimpleToggle optionState, string IDString, Vector2 pos)
        {
            FilterCheckBox checkBox = new FilterCheckBox(this, CWT.Options, optionState, pos, 0f, labelText, IDString);

            //A next selectable doesn't exist for these directions
            checkBox.nextSelectable[0] = checkBox;
            checkBox.nextSelectable[2] = checkBox;

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
