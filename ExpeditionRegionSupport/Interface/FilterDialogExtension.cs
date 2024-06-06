using ExpeditionRegionSupport.Interface.Components;
using Extensions;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Interface
{
    public static class FilterDialogExtension
    {
        public class FilterDialogCWT
        {
            public Action<FilterDialog> RunOnNextUpdate;
            public ScrollablePage Page;
            public FilterOptions Options;

            /// <summary>
            /// Indicates that the constructor has successfully run without an exception
            /// </summary>
            public bool InitSuccess;

            /// <summary>
            /// Ensures that the close dialog trigger is handled only once
            /// </summary>
            public bool PauseButtonHandled;
        }

        public static readonly ConditionalWeakTable<FilterDialog, FilterDialogCWT> filterDialogCWT = new();

        public static FilterDialogCWT GetCWT(this FilterDialog self) => filterDialogCWT.GetValue(self, _ => new());

        public static void InitializePage(this FilterDialog self)
        {
            var cwt = self.GetCWT();

            cwt.Page = new ScrollablePage(self, null, "main", 0);
            cwt.Options = new FilterOptions(self, cwt.Page, cwt.Page.pos);

            if (self is ExpeditionSettingsDialog)
                cwt.Options.MaxDividers = 3; //Only the first checkbox grouping should have dividers
            else
                cwt.Options.MaxDividers = 10;

            cwt.Page.AddSubObject(cwt.Options);

            self.dialogPage = cwt.Page;
        }

        /// <summary>
        /// Sets field values back to base values
        /// </summary>
        public static void SetToDefault(this FilterDialog self)
        {
            ScrollablePage page = self.GetCWT().Page;

            page.SetToDefault();
            self.AssignValuesFromPage();
        }

        public static void AssignValuesFromPage(this FilterDialog self)
        {
            ScrollablePage page = self.GetCWT().Page;

            self.uAlpha = page.BaseAlpha;
            self.currentAlpha = page.Alpha;
            self.lastAlpha = page.LastAlpha;
            self.targetAlpha = page.TargetAlpha;

            self.opening = page.Opening;
            self.closing = page.Closing;
        }

        public static void OpenFilterDialog(this FilterDialog self)
        {
            ScrollablePage page = self.GetCWT().Page;

            self.opening = page.Opening = true;
            self.closing = page.Closing = false;
            self.targetAlpha = page.TargetAlpha = 1f;
        }

        public static void CloseFilterDialog(this FilterDialog self, bool instantClose = false)
        {
            //Normally there is a fade out process before close. This code will bypass it.
            if (instantClose)
            {
                self.SetToDefault();

                if (self.owner != null)
                {
                    self.owner.unlocksButton.greyedOut = false;
                    self.owner.startButton.greyedOut = false;
                }

                self.pageTitle.RemoveFromContainer();
                self.manager.StopSideProcess(self);
                return;
            }

            self.Singal(null, "CLOSE");
        }

        public static void LogValues(this FilterDialog self)
        {
            if (self.closing || self.opening)
            {
                Plugin.Logger.LogInfo(self.closing ? "Closing" : "Opening");
                Plugin.Logger.LogInfo("Current Alpha: " + self.currentAlpha);
                Plugin.Logger.LogInfo("Target Alpha: " + self.targetAlpha);
            }
        }
    }
}
