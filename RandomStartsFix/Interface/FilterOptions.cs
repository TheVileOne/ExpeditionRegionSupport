using Extensions;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Interface
{
    public class FilterOptions : PositionedMenuObject, CheckBox.IOwnCheckBox
    {
        public static Color LABEL_CHECKED_COLOR = new Color(0.83f, 0.83f, 0.83f);
        public static Color LABEL_UNCHECKED_COLOR = new Color(0.25f, 0.25f, 0.25f);

        public Action<FilterCheckBox, bool> OnFilterChanged;

        public CheckBox.IOwnCheckBox OptionHandle;

        public List<FSprite> Dividers = new List<FSprite>();
        public List<MenuLabel> Filters = new List<MenuLabel>();
        public List<FilterCheckBox> Boxes = new List<FilterCheckBox>();

        /// <summary>
        /// Used to keep last divider from being rendered
        /// </summary>
        private FSprite pendingDivider;

        /// <summary>
        /// A counter that disables input handling when user clicks too soon
        /// </summary>
        public int DoubleClickProtection;

        /// <summary>
        /// A flag that indicates that user has clicked more than once in a very short period of time
        /// </summary>
        public bool DoubleClick;

        public CheckBox LastFilterChanged;

        private bool filtersDirty;

        public FilterOptions(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            OptionHandle = this;
        }

        public void AddOption(FilterCheckBox option)
        {
            Plugin.Logger.LogInfo("Added Option: " + option.label.myText);

            Boxes.Add(option);
            Filters.Add(option.label);

            onCheckBoxAdded(option);

            FSprite divider = createDividerSprite();

            if (pendingDivider != null) //Offset divider add in order to not render a divider below the last filter option
            {
                Dividers.Add(pendingDivider);
                Container.AddChild(pendingDivider);
            }

            pendingDivider = divider;
            subObjects.Add(option);
        }

        public void InsertAt(FilterCheckBox option, int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException();

            if (index >= Boxes.Count)
            {
                AddOption(option);
                return;
            }

            Boxes.Insert(index, option);
            Filters.Insert(index, option.label);

            onCheckBoxAdded(option);
            filtersDirty = true;
        }

        private void onCheckBoxAdded(FilterCheckBox box)
        {
            box.label.SetPosition(new Vector2(-250f, 14f));
            box.label.SetColor(box.Checked ? LABEL_CHECKED_COLOR : LABEL_UNCHECKED_COLOR);

            box.reportTo = OptionHandle; //Example check state handling

            if (box.Container != Container)
                Container.MoveChildrenToNewContainer(box.Container);
        }

        private FSprite createDividerSprite()
        {
            return new FSprite("pixel", true)
            {
                x = 684f - pos.x,
                y = 571f - 37f * (Dividers.Count + 1),
                scaleX = 270f,
                color = new Color(0.4f, 0.4f, 0.4f)
            };
        }

        public bool HasCheckedOptions(FilterCheckBox excludeThis)
        {
            return Boxes.Count == 1 || Boxes.Exists(o => o != excludeThis && !o.FilterImmune && o.Checked);
        }

        public bool GetChecked(CheckBox box)
        {
            return (box as FilterCheckBox).Checked;
        }

        public void SetChecked(CheckBox box, bool checkState)
        {
            Plugin.Logger.LogInfo(box.label.text + (checkState ? " filter removed" : " filter applied"));

            //Basic CheckBox validation is handled before this logic is run

            if (!(box is FilterCheckBox))
                throw new ArgumentException("FilterOptions only uses FilterCheckBox");

            FilterCheckBox filterBox = (FilterCheckBox)box;

            filterBox.SetChecked(checkState); //This is needed in case SetChecked gets called from base CheckBox before setting FilterCheckBox.Checked

            //TODO: Comments
            if (!DoubleClick && LastFilterChanged == box && DoubleClickProtection > 0)
            {
                DoubleClick = true;
                DoubleClickProtection = 100;

                menu.PlaySound(SoundID.MENU_Player_Join_Game);
                //SetChecked(box, true);
                filterBox.SetToLastState();
                return;
            }

            box.label.SetColor(checkState ? LABEL_CHECKED_COLOR : LABEL_UNCHECKED_COLOR);

            OnFilterChanged?.Invoke((FilterCheckBox)box, checkState);
            LastFilterChanged = box;
        }

        public override void Update()
        {
            base.Update();

            if (filtersDirty)
            {
                RefreshFilters();
                filtersDirty = false;
            }

            if (DoubleClickProtection > 0)
            {
                DoubleClickProtection--;
                return;
            }
            DoubleClick = false;
        }

        /// <summary>
        /// Changes all associated sprite positions according to index sorting
        /// </summary>
        public void RefreshFilters()
        {
            LastFilterChanged = null;

            for (int i = 0; i < Boxes.Count; i++)
            {
                FilterCheckBox filterOption = Boxes[i];
                MenuLabel filterLabel = filterOption.label;

                //TODO: This position assignment may be wrong
                filterOption.SetPosY(577f - 37f * i);
                filterLabel.SetPosY(590f - 37f * i);
            }

            //Check if dividers need to be updated
            int dividersNeeded = Boxes.Count - Dividers.Count - 1;

            if (dividersNeeded > 0)
            {
                pendingDivider = null;

                //Reset Y positions for all dividers
                for(int i = 0; i< Dividers.Count;  i++)
                    Dividers[i].y = 571f - 37f * (i + 1); //TODO: Check padding is correct

                //Add any extra dividers as necessary
                while (dividersNeeded > 0)
                {
                    FSprite divider = createDividerSprite();

                    Dividers.Add(divider);
                    Container.AddChild(divider);
                    dividersNeeded--;
                }

                pendingDivider = createDividerSprite(); //It is expected to there to be an FSprite stored here
            }
        }
    }

    public class FilterCheckBox: CheckBox
    {
        public FilterOptions Owner;

        /// <summary>
        /// This checkbox should be treated more like a secondary filter, and will be ignored by the primary filter criteria
        /// </summary>
        public bool FilterImmune;

        /// <summary>
        /// Stores the check state for this CheckBox.
        /// </summary>
        public new bool Checked
        {
            get; private set;
        }

        public FilterCheckBox(Menu.Menu menu, FilterOptions owner, IOwnCheckBox reportTo, Vector2 pos, float textWidth, string displayText, string IDString, bool textOnRight = false) : base(menu, owner, reportTo, pos, textWidth, displayText, IDString, textOnRight)
        {
            Owner = owner;
            Checked = true;
        }

        public FilterCheckBox(Menu.Menu menu, FilterOptions owner, IOwnCheckBox reportTo, Vector2 pos, MenuLabel label, string IDString) : this(menu, owner, reportTo, pos, 0, string.Empty, IDString)
        {
            this.label.menu = null;
            this.label.owner = null;

            RemoveSubObject(this.label);
            this.AddSubObject(this.label = label);
        }

        public override void Clicked()
        {
            //Check if this option is enabled, and is allowed to be checked on/off
            if (buttonBehav.greyedOut || (!FilterImmune && !Owner.HasCheckedOptions(this))) return;

            //Invoking base will update the base Checked state, which will notify FilterOptions of the state change.
            //This may not end up actually changing the state due to double-click protection, but currently will be set anyways.
            base.Clicked();
        }

        /// <summary>
        /// Set the value of Checked
        /// </summary>
        public void SetChecked(bool checkState)
        {
            Plugin.Logger.LogInfo("Check state set to " + checkState);
            Checked = checkState;
        }

        public void SetToLastState()
        {
            SetChecked(!Checked);
        }
    }
}
