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
    public class ScrollablePage : Page
    {
        private float baseAlpha;

        /// <summary>
        /// Returns the alpha value used for updating scroll position multiplied by a factor value.
        /// This reflects the active alpha for children sprites. 
        /// </summary>
        public float Alpha => baseAlpha * 0.95f;
        public float BaseAlpha => baseAlpha;
        public float LastAlpha;
        public float CurrentAlpha;
        public float TargetAlpha = 1f;
        public float ScrollOffset = 100f;

        /// <summary>
        /// The process of moving the page from its default position to its viewable position
        /// </summary>
        public bool Opening;

        /// <summary>
        /// The process of moving the page from its viewable position to its default position
        /// </summary>
        public bool Closing;

        /// <summary>
        /// A flag that indicates that the page has successfully closed.
        /// </summary>
        public bool HasClosed;

        public ScrollablePage(Menu.Menu menu, MenuObject owner, string name, int index) : base(menu, owner, name, index)
        {
        }

        public override void Update()
        {
            base.Update();
            LastAlpha = CurrentAlpha;
            CurrentAlpha = Mathf.Lerp(CurrentAlpha, TargetAlpha, 0.2f);

            if (Opening)
            {
                HasClosed = false;

                if (pos.y <= 0.01f)
                    Opening = false;
            }

            if (Closing && Math.Abs(CurrentAlpha - TargetAlpha) < 0.09f)
            {
                HasClosed = true;
                Closing = false;
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker); //subObjects GrafUpdate gets handled in base

            if (Opening || Closing)
            {
                baseAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(LastAlpha, CurrentAlpha, timeStacker)), 1.5f);

                foreach (MenuObject child in subObjects)
                    child.SetAlpha(Alpha);
            }

            pos.y = Mathf.Lerp(menu.manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (baseAlpha < 0.999f) ? baseAlpha : 1f);
        }
    }
}
