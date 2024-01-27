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
        public const int DEFAULT_CHALLENGE_COUNT = 9;

        //public CheckBox.IOwnCheckBox OptionHandle;

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
        }

        public void AddOption(FilterCheckBox option)
        {
            Boxes.Add(option);
            Filters.Add(option.label);

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

            filtersDirty = true;
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
            return box.Checked;
        }

        public void SetChecked(CheckBox box, bool checkState)
        {
            if (box.buttonBehav.greyedOut) return;

            //TODO: Comments
            if (!DoubleClick && LastFilterChanged == box && DoubleClickProtection > 0)
            {
                DoubleClick = true;
                DoubleClickProtection = 100;

                menu.PlaySound(SoundID.MENU_Player_Join_Game);
                //SetChecked(box, true);
            }

            box.Checked = checkState;
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

                filterOption.pos.y = filterOption.lastPos.y = 577f - 37f * i;
                filterLabel.pos.y = filterLabel.lastPos.y = 590f - 37f * i;
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
        /// <summary>
        /// This checkbox should be treated more like a secondary filter, and will be ignored by the primary filter criteria
        /// </summary>
        public bool FilterImmune;

        public FilterCheckBox(Menu.Menu menu, FilterOptions owner, IOwnCheckBox reportTo, Vector2 pos, float textWidth, string displayText, string IDString, bool textOnRight = false) : base(menu, owner, reportTo, pos, textWidth, displayText, IDString, textOnRight)
        {
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
            if (buttonBehav.greyedOut || (!FilterImmune && !(owner as FilterOptions).HasCheckedOptions(this))) return;

            base.Clicked();
        }
    }
}
