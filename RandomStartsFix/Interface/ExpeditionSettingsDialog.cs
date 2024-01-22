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
    public partial class ExpeditionSettingsDialog : FilterDialog
    {
        public ExtensionMethods.FilterDialogCWT CWT;
        public ScrollablePage MainPage
        {
            get => CWT.Page;
            set => CWT.Page = value;
        }

        private CheckBox shelterDetectionCheckBox;
        private SimpleButton reloadButton;

        //private RegionFilter regionFilter; //TODO: Create?

        private CheckboxCollection filterOptions;
        private CheckBox regionFilterVanilla;
        private CheckBox regionFilterMoreSlugcats;
        private CheckBox regionFilterCustom;
        private CheckBox regionFilterVisitedOnly;

        private List<MenuLabel> filters;

        private string closeButtonText = "CLOSE";
        private string closeButtonSignal => closeButtonText;

        //TODO:
        //Show custom regions available in Expedition?

        public ExpeditionSettingsDialog(ProcessManager manager, ChallengeSelectPage owner) : base(manager, owner)
        {
            CWT = this.GetCWT();
            float num = 500;
            float globalOffX = 200;//(num - 250f) / -2f;

            //RoundedRect roundedRect = new RoundedRect(this, pages[0], new Vector2(243f + globalOffX, 100f), new Vector2(num, 550f), true);
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == closeButtonSignal)
            {
                PlaySound(SoundID.MENU_Switch_Page_Out);
                closing = true;
                targetAlpha = 0f;
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
                Singal(null, closeButtonSignal);
                pauseButtonHandled = true;
            }

            base.Update();
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

            dividers = new List<FSprite>();
            filters = new List<MenuLabel>();
            checkBoxes = new List<CheckBox>();

            initializeCheckBoxes();

            MainPage.subObjects.Add(CWT.Options); //TODO: handle this differently?

            this.opening = true;
            this.targetAlpha = 1f;
        }

        private void initializeHeaders()
        {
            pageTitle = new FSprite("filters", true);
            pageTitle.SetAnchor(0.5f, 0f);
            pageTitle.SetPosition(720f, 680f);

            MainPage.Container.AddChild(pageTitle);

            if (manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
            {
                localizedSubtitle = new MenuLabel(this, MainPage, Translate("-SETTINGS-"), new Vector2(683f, 740f), default(Vector2), false, null);
                localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
                MainPage.subObjects.Add(localizedSubtitle);
            }

            heading = new MenuLabel(this, MainPage, Translate("REGION FILTER"), new Vector2(683f, 655f), default(Vector2), false, null);
            heading.label.color = new Color(0.7f, 0.7f, 0.7f);
            MainPage.subObjects.Add(heading);
            description = new MenuLabel(this, MainPage, Translate("Toggle which challenges can appear when randomising"), new Vector2(683f, 635f), default(Vector2), false, null);
            description.label.color = new Color(0.7f, 0.7f, 0.7f);
            MainPage.subObjects.Add(description);
        }

        private void initializeCancelButton()
        {
            string closeButtonTranslation = Translate(closeButtonText);

            //Adjust button width to accomodate varying translation lengths
            float num = 85f;
            float num2 = Menu.Remix.MixedUI.LabelTest.GetWidth(closeButtonTranslation, false) + 10f;
            if (num2 > num)
                num = num2;

            cancelButton = new SimpleButton(this, MainPage, closeButtonTranslation, closeButtonSignal, new Vector2(683f - num / 2f, 120f), new Vector2(num, 35f));
            cancelButton.nextSelectable[0] = cancelButton;
            cancelButton.nextSelectable[2] = cancelButton;

            MainPage.subObjects.Add(cancelButton);
        }

        private void initializeCheckBoxes()
        {
            regionFilterVanilla = CreateCheckBox("VANILLA REGIONS", 0, "VANILLA");
            regionFilterMoreSlugcats = CreateCheckBox("MORE SLUGCATS REGIONS", 1, "MORE SLUGCATS");
            regionFilterCustom = CreateCheckBox("CUSTOM REGIONS", 2, "MORE SLUGCATS");
            regionFilterVisitedOnly = CreateCheckBox("VISITED REGIONS ONLY", 3, "VISITED ONLY", true);
        }

        public CheckBox CreateCheckBox(string labelText, int checkBoxIndex, string checkBoxIDString, bool isLastCheckBox = false)
        {
            Vector2 defaultLabelPosition = new Vector2(553, 590);
            Vector2 defaultCheckBoxPosition = new Vector2(793, 577);
            Vector2 defaultDividerPosition = new Vector2(684 - leftAnchor, 571);

            float checkBoxHeight = 37f * checkBoxIndex; //This affects how checkboxes stack 

            Vector2 actualLabelPosition = new Vector2(defaultLabelPosition.x, defaultLabelPosition.y - checkBoxHeight);
            Vector2 actualCheckBoxPosition = new Vector2(defaultCheckBoxPosition.x, defaultCheckBoxPosition.y - checkBoxHeight);
            Vector2 actualDividerPosition = new Vector2(defaultDividerPosition.x, defaultDividerPosition.y - checkBoxHeight);

            MenuLabel checkBoxLabel = new MenuLabel(this, MainPage, labelText, actualLabelPosition, default(Vector2), true, null);
            checkBoxLabel.label.alignment = FLabelAlignment.Left;

            filters.Add(checkBoxLabel);
            MainPage.subObjects.Add(checkBoxLabel);
            CheckBox checkBox = new CheckBox(this, MainPage, this, actualCheckBoxPosition, 0f, string.Empty, checkBoxIDString, false);
            
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
                FSprite dividerSprite = new FSprite("pixel", true);

                dividerSprite.SetPosition(actualDividerPosition);
                dividerSprite.scaleX = 270f;
                dividerSprite.color = new Color(0.4f, 0.4f, 0.4f);
                container.AddChild(dividerSprite);
                dividers.Add(dividerSprite);
            }

            MainPage.subObjects.Add(checkBox);
            checkBoxes.Add(checkBox);
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
            MainPage = new ScrollablePage(this, null, "main", 0);

            pages.Add(MainPage);
            pos = new Vector2((manager.rainWorld.options.ScreenSize.x - size.x) * 0.5f, (manager.rainWorld.options.ScreenSize.x - this.size.y) * 0.5f);
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

            MainPage.Container.AddChild(darkSprite);
        }
    }
}
