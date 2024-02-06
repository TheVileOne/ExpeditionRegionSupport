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

        #region InitStrings

        public readonly string HeaderText = "SETTINGS";
        public readonly string SubHeaderText = "Region filter";

        private string closeButtonText = "CLOSE";

        #endregion

        private CheckBox shelterDetectionCheckBox;
        private SimpleButton reloadButton;

        //private RegionFilter regionFilter; //TODO: Create?

        private CheckBox regionFilterVanilla;
        private CheckBox regionFilterMoreSlugcats;
        private CheckBox regionFilterCustom;
        private CheckBox regionFilterVisitedOnly;

        //TODO:
        //Show custom regions available in Expedition?

        public ExpeditionSettingsDialog(ProcessManager manager, ChallengeSelectPage owner) : base(manager, owner)
        {
            heading.text = "REGION FILTER";
            description.text = "Filter spawnable region categories when randomising";
        }

        public void ReloadFiles()
        {
            ModMerger modMerger = new ModMerger();

            //TODO
        }       

        private void initializeDialog()
        {
            initializeBase();

            float[] screenOffsets = RWCustom.Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            rightAnchor = screenOffsets[1];

            //Move to scroll start positions
            MainPage.pos.y = MainPage.pos.y + 2000f;

            initializeHeaders();
            initializeCancelButton();

            InitializeCheckBoxes();

            opening = true;
            targetAlpha = 1f;
        }

        private void initializeHeaders()
        {
            pageTitle = new FSprite("filters", true);
            pageTitle.SetAnchor(0.5f, 0f);
            pageTitle.SetPosition(720f, 680f);

            MainPage.Container.AddChild(pageTitle);

            if (manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
            {
                localizedSubtitle = new MenuLabel(this, MainPage, Translate("-SETTINGS-"), new Vector2(683f, 740f), default, false, null);
                localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
                MainPage.subObjects.Add(localizedSubtitle);
            }

            heading = new MenuLabel(this, MainPage, Translate("REGION FILTER"), new Vector2(683f, 655f), default, false, null);
            heading.label.color = new Color(0.7f, 0.7f, 0.7f);
            MainPage.subObjects.Add(heading);
            description = new MenuLabel(this, MainPage, Translate("Toggle which regions are spawnable"), new Vector2(683f, 635f), default, false, null);
            description.label.color = new Color(0.7f, 0.7f, 0.7f);
            MainPage.subObjects.Add(description);
        }

        private void initializeCancelButton()
        {
            string closeButtonTranslation = Translate(closeButtonText);

            //Adjust button width to accomodate varying translation lengths
            float num = Math.Max(85f, Menu.Remix.MixedUI.LabelTest.GetWidth(closeButtonTranslation, false) + 10f);

            //cancelButton = new SimpleButton(this, MainPage, closeButtonTranslation, closeButtonSignal, new Vector2(683f - num / 2f, 120f), new Vector2(num, 35f));
            cancelButton.nextSelectable[0] = cancelButton;
            cancelButton.nextSelectable[2] = cancelButton;

            MainPage.subObjects.Add(cancelButton);
        }

        public void InitializeCheckBoxes()
        {
            regionFilterVanilla = CreateCheckBox("Vanilla Regions", 0, "VANILLA");
            regionFilterMoreSlugcats = CreateCheckBox("More Slugcats Regions", 1, "MORE SLUGCATS");
            regionFilterCustom = CreateCheckBox("Custom Regions", 2, "MORE SLUGCATS");
            regionFilterVisitedOnly = CreateCheckBox("Visited Regions Only", 3, "VISITED ONLY", true);
        }

        public CheckBox CreateCheckBox(string labelText, int checkBoxIndex, string checkBoxIDString, bool isLastCheckBox = false)
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

            FilterCheckBox checkBox = new FilterCheckBox(this, CWT.Options, actualCheckBoxPosition, 0f, string.Empty, checkBoxIDString, false)
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

        private void initializeBase()
        {
            initializeMenuBase();
            initializeDialogBase();
        }

        private void initializeMenuBase()
        {
            pages = new List<Page>();
            container = new FContainer();
            Futile.stage.AddChild(container);
            mySoundLoopName = string.Empty;
            cursorContainer = new FContainer();
            currentPage = 0;

            infoLabel = new FLabel(RWCustom.Custom.GetFont(), string.Empty);
            infoLabel.y = Mathf.Max(0.01f + manager.rainWorld.options.SafeScreenOffset.y, 20.01f);
            infoLabel.x = manager.rainWorld.screenSize.x / 2f + 0.01f;
            Futile.stage.AddChild(infoLabel);

            UpdateInfoText();
        }

        private void initializeDialogBase()
        {
            pages.Add(MainPage);
            pos = new Vector2((manager.rainWorld.options.ScreenSize.x - size.x) * 0.5f, (manager.rainWorld.options.ScreenSize.x - size.y) * 0.5f);
            darkSprite = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                anchorX = 0f,
                anchorY = 0f,
                scaleX = manager.rainWorld.screenSize.x + 2f,
                scaleY = manager.rainWorld.screenSize.x + 2f,
                x = -1f,
                y = -1f,
                alpha = 0f
            };

            container.AddChild(darkSprite);
        }
    }
}
