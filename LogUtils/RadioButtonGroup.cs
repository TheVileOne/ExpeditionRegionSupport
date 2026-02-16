using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    public class RadioButtonGroup : MenuObject, CheckBox.IOwnCheckBox
    {
        public event Events.EventHandler<RadioButtonGroup, SelectionChangedEventArgs> SelectionChanged;

        public CheckBox Selected { get; private set; }

        /// <summary>
        /// The option set by default (one option has to be)
        /// </summary>
        internal CheckBox DefaultOption { get; private set; }

        /// <summary>
        /// The height of a <see cref="CheckBox"/> sprite
        /// </summary>
        public const float OPTION_HEIGHT = 24f;

        /// <summary>
        /// The distance between listed options
        /// </summary>
        public const float OPTION_PADDING = 10f;

        /// <summary>
        /// The position of the <see cref="RadioButtonGroup"/> options.
        /// </summary>
        public readonly Vector2 Position;

        internal List<CheckBox> Options = new List<CheckBox>();

        /// <summary>
        /// Contains options belonging to <see langword="this"/> instance
        /// </summary>
        public IReadOnlyList<CheckBox> OptionCollection => Options;

        public float Height => (OPTION_HEIGHT + OPTION_PADDING) * Options.Count;

        public RadioButtonGroup(Menu.Menu menu, MenuObject owner, Vector2 position) : base(menu, owner)
        {
            Position = position;
        }

        /// <summary>
        /// Selects an option by its identifying string
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="message">Case-sensitive identifier</param>
        public override void Singal(MenuObject sender, string message)
        {
            CheckBox found = Options.Find(o => o.IDString == message);

            if (found != null)
            {
                found.Checked = true;
                return;
            }
            base.Singal(sender, message);
        }

        /// <summary>
        /// Adds a new <see cref="RadioButtonGroup"/> option
        /// </summary>
        /// <param name="textWidth">The hover range for this option</param>
        /// <param name="displayText">The text that will show on this option's display label</param>
        /// <param name="optionID">An identifying string for a <see cref="RadioButtonGroup"/> option; can be selectable through <see cref="MenuObject.Singal"/></param>
        public void AddOption(float textWidth, string displayText, string optionID)
        {
            Vector2 optionPos = new Vector2(Position.x, Position.y - Height);
            CheckBox option = new CheckBox(menu, this, this, optionPos, textWidth, displayText, optionID, textOnRight: true);

            subObjects.Add(option);
            Options.Add(option);
        }

        /// <summary>
        /// Removes a <see cref="RadioButtonGroup"/> option with the specified identifying string
        /// </summary>
        /// <param name="optionID">An identifying string for a <see cref="RadioButtonGroup"/> option/param>
        public void RemoveOption(string optionID)
        {
            CheckBox found = Options.Find(o => o.IDString == optionID);

            if (found == null)
            {
                UtilityLogger.LogWarning("Option not found");
                return;
            }

            subObjects.Remove(found);
            Options.Remove(found);
            if (found == Selected) //When removing a selected option, a new option must be selected
                Selected = Options.Count > 0 ? Options[0] : null;
        }

        public void SetInitial(string optionID)
        {
            CheckBox found = Options.Find(o => o.IDString == optionID);

            if (found == null)
            {
                UtilityLogger.LogWarning("Option not found");
                return;
            }
            Selected = DefaultOption = found;
        }

        bool CheckBox.IOwnCheckBox.GetChecked(CheckBox box)
        {
            if (Selected == null)
                throw new InvalidOperationException("No option is selected");

            return Selected == box;
        }

        void CheckBox.IOwnCheckBox.SetChecked(CheckBox box, bool checkState)
        {
            if (checkState && box != Selected && Options.Contains(box))
            {
                CheckBox lastSelected = Selected;
                Selected = box;
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(lastSelected, box));
            }
        }
    }

    public class SelectionChangedEventArgs(CheckBox lastSelection, CheckBox newSelection) : EventArgs
    {
        public CheckBox LastSelection = lastSelection;
        public CheckBox NewSelection = newSelection;
    }
}
