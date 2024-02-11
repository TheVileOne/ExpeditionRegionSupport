using ExpeditionRegionSupport.Settings;
using Extensions;
using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = UnityEngine.Vector2;

namespace ExpeditionRegionSupport.Interface.Components
{
    public abstract class MenuObjectFactory<T> where T : MenuObject
    {
        /// <summary>
        /// Spaces objects using a uniform distance defined by the spacer
        /// </summary>
        public PositionSpacer Spacer;

        /// <summary>
        /// A position that will be used as an alternative to a Spacer position. Will be used when Spacer is null
        /// </summary>
        public Vector2 Position;

        public Menu.Menu Menu;
        public MenuObject Owner;

        public List<T> ObjectsCreated;
        protected event Action<T> ObjectCreated;

        private bool firstHandled = false;

        public MenuObjectFactory(Menu.Menu menu, MenuObject owner, List<T> listStorage, Action<T> onObjectCreated)
        {
            Menu = menu;
            Owner = owner;

            ObjectsCreated = listStorage ?? new List<T>();

            /*
             * Two events need to be handled on creation of a new object
             * 
             * I.  Selectables for left/right control navigation need to be self referenced
             * II. The object needs to be added to a list, either through a conventional way or a custom way provided through a parameter
             */
            ObjectCreated += onObjectCreated ?? ObjectsCreated.Add;
        }

        /// <summary>
        /// Gets the next position determined by the spacer, or Position when a spacer is not defined
        /// </summary>
        protected Vector2 GetNewPosition()
        {
            if (Spacer != null)
                return firstHandled ? Spacer.NextPosition : Spacer.CurrentPosition;
            return Position;
        }

        protected virtual void OnObjectCreated(MenuObject menuObject)
        {
            firstHandled = true;

            menuObject.InitializeSelectionNav();
            ObjectCreated.Invoke((T)menuObject);
        }
    }

    public class SimpleButtonFactory : MenuObjectFactory<SimpleButton>
    {
        public const float BUTTON_HEIGHT = 35f;

        public SimpleButtonFactory(Menu.Menu menu, MenuObject owner, List<SimpleButton> listStorage = null, Action<SimpleButton> onObjectCreated = null) : base(menu, owner, listStorage, onObjectCreated)
        {
        }

        public SimpleButton Create(string buttonTextRaw, string signalText)
        {
            return _Create(buttonTextRaw, signalText, GetNewPosition());
        }

        public SimpleButton Create(string buttonTextRaw, string signalText, Vector2 pos)
        {
            return _Create(buttonTextRaw, signalText, pos);
        }

        protected SimpleButton _Create(string buttonTextRaw, string signalText, Vector2 pos)
        {
            string buttonTextTranslated = Menu.Translate(buttonTextRaw);

            //Adjust button width to accomodate varying translation lengths
            float buttonWidth = Math.Max(85f, LabelTest.GetWidth(buttonTextTranslated, false) + 10f); //+10 is the padding

            //Creates a button aligned vertically in the center of the screen
            SimpleButton button = new SimpleButton(Menu, Owner, buttonTextTranslated, signalText,
                         new Vector2(pos.x - (buttonWidth / 2f), pos.y), new Vector2(buttonWidth, BUTTON_HEIGHT)); //pos, size

            OnObjectCreated(button);
            return button;
        }
    }

    public class FilterCheckBoxFactory : MenuObjectFactory<FilterCheckBox>
    {
        public FilterCheckBoxFactory(Menu.Menu menu, MenuObject owner, List<FilterCheckBox> listStorage = null, Action<FilterCheckBox> onObjectCreated = null) : base(menu, owner, listStorage, onObjectCreated)
        {
        }

        public FilterCheckBox Create(string labelText, SimpleToggle optionState, string IDString, Vector2 pos)
        {
            return _Create(labelText, optionState, IDString, pos);
        }

        public FilterCheckBox Create(string labelText, SimpleToggle optionState, string IDString)
        {
            return _Create(labelText, optionState, IDString, GetNewPosition());
        }

        protected FilterCheckBox _Create(string labelText, SimpleToggle optionState, string IDString, Vector2 pos)
        {
            FilterCheckBox checkBox = new FilterCheckBox(Menu, (FilterOptions)Owner, optionState, pos, 0f, labelText, IDString);

            OnObjectCreated(checkBox);
            return checkBox;
        }
    }
}
