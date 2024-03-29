﻿using ExpeditionRegionSupport.Interface.Components;
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

        public event Action<ExpeditionSettingsDialog> OnDialogClosed;

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
            SimpleButtonFactory factory = new SimpleButtonFactory(this, MainPage)
            {
                Spacer = new PositionSpacer(new Vector2(683f, 265f), SimpleButtonFactory.BUTTON_HEIGHT, 10f)
            };

            factory.ObjectCreated += (b) => b.GetCWT().CenterInParent = true; //Keeps button position correct on resize

            reloadFromFileButton = factory.Create("Reload Expedition Files", ExpeditionConsts.Signals.RELOAD_MOD_FILES);
            cancelChangesButton = factory.Create("Restore Defaults", ExpeditionConsts.Signals.RESTORE_DEFAULTS);
            customizeSpawnsButton = factory.Create("Customize Spawns", ExpeditionConsts.Signals.OPEN_SPAWN_DIALOG);

            if (!Plugin.DebugMode)
            {
                reloadFromFileButton.buttonBehav.greyedOut = true;
                customizeSpawnsButton.buttonBehav.greyedOut = true;
            }

            List<SimpleButton> buttons = factory.ObjectsCreated;
            
            buttons.ForEach(MainPage.AddSubObject);

            MenuUtils.UpdateButtonSize(buttons, cancelButton);
            MenuUtils.SetSelectables(buttons, CWT.Options.Boxes.Last(), cancelButton);
        }

        public void InitializeCheckBoxes()
        {
            FilterOptions options = CWT.Options;

            FilterCheckBoxFactory factory = new FilterCheckBoxFactory(this, options, options.Boxes, options.AddOption)
            {
                Spacer = new PositionSpacer(new Vector2(793f, 577f), FilterCheckBoxFactory.CHECKBOX_HEIGHT, 2f)
            };

            regionFilterVanilla = factory.Create("Vanilla Regions", ExpeditionSettings.Filters.AllowVanillaRegions, "VANILLA");
            regionFilterMoreSlugcats = factory.Create("More Slugcats Regions", ExpeditionSettings.Filters.AllowMoreSlugcatsRegions, "MORE SLUGCATS");
            regionFilterCustom = factory.Create("Custom Regions", ExpeditionSettings.Filters.AllowCustomRegions, "CUSTOM");
            regionFilterVisitedOnly = factory.Create("Visited Regions Only", ExpeditionSettings.Filters.VisitedRegionsOnly, "VISITED ONLY");

            Vector2 nextPos = factory.Spacer.NextPosition - new Vector2(0, 80f);

            shelterDetectionCheckBox = factory.Create("Enable Shelter Detection", ExpeditionSettings.DetectShelterSpawns, "SHELTER_DETECTION", nextPos);

            regionFilterVisitedOnly.FilterImmune = true;
            shelterDetectionCheckBox.FilterImmune = true;

            MenuUtils.SetSelectables(options.Boxes, cancelButton);
        }

        public override void Singal(MenuObject sender, string signal)
        {
            switch (signal)
            {
                case ExpeditionConsts.Signals.RESTORE_DEFAULTS:
                    {
                        ExpeditionSettings.RestoreToDefaults();
                        return;
                    }
                case ExpeditionConsts.Signals.RELOAD_MOD_FILES:
                    {
                        ReloadFiles();
                        return;
                    }
                case ExpeditionConsts.Signals.OPEN_SPAWN_DIALOG:
                    {
                        //Not implemented yet
                        return;
                    }
            }

            base.Singal(sender, signal);

            if (signal == "CLOSE")
                OnDialogClosed?.Invoke(this);
        }
    }
}
