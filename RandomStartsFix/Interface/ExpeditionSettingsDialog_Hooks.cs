using Extensions;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Interface
{
    public partial class ExpeditionSettingsDialog
    {
        public static void ApplyHooks()
        {
            IL.Menu.Dialog.ctor_ProcessManager += Dialog_ctor_ProcessManager;
            
            On.Menu.FilterDialog.ctor += FilterDialog_ctor;
            IL.Menu.FilterDialog.ctor += FilterDialog_ctor;

            On.Menu.FilterDialog.Update += FilterDialog_Update;
            IL.Menu.FilterDialog.Update += FilterDialog_Update;
            
            IL.Menu.FilterDialog.GrafUpdate += FilterDialog_GrafUpdate;

            On.Menu.FilterDialog.GetChecked += FilterDialog_GetChecked;
            On.Menu.FilterDialog.SetChecked += FilterDialog_SetChecked;

            On.Menu.FilterDialog.Singal += FilterDialog_Singal;

            On.Menu.CheckBox.ctor += CheckBox_ctor;
            IL.Menu.Page.ctor += Page_ctor;
        }

        #region Constructor hooks

        private static void Page_ctor(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchStfld<Page>(nameof(Page.mouseCursor))); //1st reference to mouseCursor
            cursor.GotoPrev(MoveType.Before, //The start of argument construction for mouseCursor
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdarg(0));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Page, bool>>((p) => p is ScrollablePage); //Skip over mouseCursor creation. It breaks scrollable pages
            cursor.BranchTo(OpCodes.Brtrue, MoveType.After,
                x => x.MatchLdfld<Page>(nameof(Page.mouseCursor)),
                x => x.Match(OpCodes.Callvirt));
        }

        /// <summary>
        /// This hook allows FilterDialog to use a ScrollablePage instead of a Page
        /// </summary>
        private static void Dialog_ctor_ProcessManager(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchNewobj(typeof(Page))); //Go to Page instantiation
            cursor.Emit(OpCodes.Ldarg_0); //Put `this` onto stack
            cursor.EmitDelegate(replacePage);
        }

        private static void FilterDialog_ctor(On.Menu.FilterDialog.orig_ctor orig, FilterDialog self, ProcessManager manager, ChallengeSelectPage owner)
        {
            var cwt = self.GetCWT();

            self.InitializePage();

            if (self is ExpeditionSettingsDialog)
            {
                self.owner = owner;
                self.manager = manager;
                self.ID = ProcessManager.ProcessID.Dialog;
                (self as ExpeditionSettingsDialog).initializeDialog();
                onFilterDialogCreated(self);
            }
            else //orig isn't called to avoid Challenge filter specific logic in constructor
            {
                orig(self, manager, owner);
            }

            self.opening = cwt.Page.Opening = true;
            self.targetAlpha = cwt.Page.TargetAlpha = 1f;
        }

        private static void FilterDialog_ctor(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, //This is just after label is added to this.challengeTypes
                x => x.MatchLdloc(7),
                x => x.Match(OpCodes.Callvirt));

            //First subObjects branch over

            cursor.BranchTo( //Branch past second reference to label
                x => x.MatchLdloc(7),
                x => x.Match(OpCodes.Callvirt));

            //Replace CheckBox

            cursor.GotoNext(MoveType.After, x => x.MatchNewobj(typeof(CheckBox))); //Go to CheckBox instantiation
            cursor.Emit(OpCodes.Ldloc, 7); //Get MenuLabel
            cursor.EmitDelegate(replaceCheckBox); //Takes a CheckBox, and MenuLabel as arguments

            //Second subObjects branch over

            cursor.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.Menu>(nameof(pages)));

            cursor.BranchTo(
                x => x.MatchLdloc(8),
                x => x.Match(OpCodes.Callvirt));

            //Branch over divider handling

            cursor.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.Menu>(nameof(container)));

            cursor.BranchTo(
                x => x.MatchLdfld<FilterDialog>(nameof(dividers)),
                x => x.MatchLdloc(9),
                x => x.Match(OpCodes.Callvirt));

            //Evaluate filters (before post-method hooking)

            cursor.GotoNext(MoveType.Before, x => x.MatchRet());
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(onFilterDialogCreated);
        }

        /// <summary>
        /// This hook isolates the sprite container for a checkbox belonging to a FilterDialog. 
        /// This makes it easier to retrieve any transfer CheckBox sprites into the FilterCheckBox class
        /// </summary>
        private static void CheckBox_ctor(On.Menu.CheckBox.orig_ctor orig, CheckBox self, Menu.Menu menu, MenuObject owner, CheckBox.IOwnCheckBox reportTo, UnityEngine.Vector2 pos, float textWidth, string displayText, string IDString, bool textOnRight)
        {
            self.menu = menu;
            self.owner = owner;

            if (menu is FilterDialog && !(self is FilterCheckBox))
                self.Container = new FContainer();
            orig(self, menu, owner, reportTo, pos, textWidth, displayText, IDString, textOnRight);
        }

        private static Page replacePage(Page page, Dialog dialog)
        {
            FilterDialog fd = dialog as FilterDialog;

            if (fd != null)
                page = fd.GetCWT().Page;

            return page;
        }

        private static FilterCheckBox replaceCheckBox(CheckBox box, MenuLabel label)
        {
            FilterOptions filterOptions = ((FilterDialog)box.menu).GetCWT().Options;

            FilterCheckBox filterBox = new FilterCheckBox(box.menu, filterOptions, box.reportTo, box.pos, label, box.IDString);

            //Do some cleanup of stuff that was handled in CheckBox constructor
            box.owner.RecursiveRemoveSelectables(box);
            box.Container.MoveChildrenToNewContainer(filterBox.Container);

            filterOptions.AddOption(filterBox);
            return filterBox;
        }

        public static void onFilterDialogCreated(FilterDialog dialog)
        {
            var cwt = dialog.GetCWT();

            cwt.Page.mouseCursor = new MouseCursor(dialog, cwt.Page, new UnityEngine.Vector2(-100f, -100f));
            cwt.Page.subObjects.Add(cwt.Page.mouseCursor);

            //Replace the reference, so that mods will add to the new reference instead
            dialog.dividers = cwt.Options.Dividers;
            dialog.challengeTypes = cwt.Options.Filters;

            if (!(dialog is ExpeditionSettingsDialog))
            {
                cwt.RunOnNextUpdate += (dialog) =>
                {
                    int filtersProcessed = cwt.Options.Filters.Count;
                    int newFiltersDetected = dialog.checkBoxes.Count - filtersProcessed;

                    //These must be modded filters if this is true
                    if (newFiltersDetected > 0)
                    {
                        UnityEngine.Debug.Log(string.Format("Detected {0} new filters in post-processing", newFiltersDetected));

                        int[] filterIndexes = new int[newFiltersDetected];

                        int currentIndex = dialog.checkBoxes.Count - 1;
                        int depositIndex = filterIndexes.Length - 1; 
                        while (newFiltersDetected > 0 && currentIndex >= 0)
                        {
                            //Fill missing indexes so that the earliest are positioned first
                            if (!cwt.Options.Boxes.Contains(dialog.checkBoxes[currentIndex]))
                            {
                                filterIndexes[depositIndex] = currentIndex;
                                depositIndex--;
                                newFiltersDetected--;
                            }

                            currentIndex--;
                        }

                        //Index position should be the same for all lists
                        foreach (int filterIndex in filterIndexes)
                        {
                            CheckBox filterBox = dialog.checkBoxes[filterIndex];
                            MenuLabel filterLabel = dialog.challengeTypes[filterIndex];

                            FilterCheckBox filterOption = replaceCheckBox(filterBox, filterLabel);

                        }
                    };
                };
            }
        }

        #endregion

        #region Update hooks

        /// <summary>
        /// This hook replaces dialog field references in FilterDialog.Update with page references
        /// </summary>
        private static void FilterDialog_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Menu.Menu>(nameof(Update)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(updateHook); //Logic here cannot be handled with vanilla logic anymore. Use custom handling

            cursor.BranchTo(OpCodes.Br, MoveType.Before, //Branch to cancel button handling
                x => x.Match(OpCodes.Ldarg_0),
                x => x.MatchLdfld<FilterDialog>(nameof(cancelButton)));
        }

        private static void updateHook(FilterDialog dialog)
        {
            var cwt = dialog.GetCWT();

            if (cwt.Page.HasClosed)
            {
                dialog.CloseFilterDialog();
                cwt.Page.HasClosed = false;
            }
        }

        private static void FilterDialog_Update(On.Menu.FilterDialog.orig_Update orig, FilterDialog self)
        {
            var cwt = self.GetCWT();

            //Set pre-existing fields now maintained through the main page. These are kept for compatibility reasons.
            self.currentAlpha = cwt.Page.CurrentAlpha;
            self.targetAlpha = cwt.Page.TargetAlpha;
            self.closing = cwt.Page.Closing;
            self.opening = cwt.Page.Opening;

            if (cwt.RunOnNextUpdate != null)
            {
                cwt.RunOnNextUpdate.Invoke(self);
                cwt.RunOnNextUpdate = null;
            }

            Plugin.Logger.LogInfo(self.currentAlpha);
            Plugin.Logger.LogInfo(self.targetAlpha);
            Plugin.Logger.LogInfo(self.closing);
            Plugin.Logger.LogInfo(self.opening);

            orig(self);
        }

        /// <summary>
        /// This hook processes base.GrafUpdate(), and maintains the alpha for darkSprite
        /// </summary>
        private static void FilterDialog_GrafUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Call));

            //Retrieve alpha value processed in Page.GrafUpdate()
            //cursor.EmitDelegate<Action<FilterDialog>>(fd => fd.uAlpha = fd.GetCWT().Page.Alpha); 

            //Check if we are either opening, or closing
            /*cursor.Emit(OpCodes.Ldfld, nameof(FilterDialog.opening));
            cursor.Emit(OpCodes.Ldfld, nameof(FilterDialog.closing));
            cursor.Emit(OpCodes.Or);

            int cursorIndex = cursor.Index;
            */
            cursor.Emit(OpCodes.Ldarg_0); //Push `this` onto stack
            cursor.EmitDelegate(updateDarkSpriteAlpha);
            /*cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, nameof(Dialog.darkSprite)); //Get darkSprite field
            cursor.Emit(OpCodes.Call, typeof(FNode).GetMethod("set_alpha"));

            //Branch to return
            ILLabel branchTarget = cursor.DefineLabel();
            cursor.MarkLabel(branchTarget);

            cursor.Emit(OpCodes.Ret); //Return early to avoid handling alpha sets

            //Establish branch
            cursor.Index = cursorIndex;
            cursor.Emit(OpCodes.Brfalse, branchTarget);*/

            cursor.Emit(OpCodes.Ret); //Return early to avoid handling alpha sets
        }

        private static void updateDarkSpriteAlpha(FilterDialog dialog)
        {
            var cwt = dialog.GetCWT();

            if (cwt.Page.Opening || cwt.Page.Closing)
            {
                dialog.uAlpha = cwt.Page.BaseAlpha; //This is here for mainly compatibility reasons
                dialog.darkSprite.alpha = cwt.Page.Alpha;
            }
        }

        #endregion

        private static void FilterDialog_Singal(On.Menu.FilterDialog.orig_Singal orig, FilterDialog self, MenuObject sender, string message)
        {
            orig(self, sender, message);

            if (message == "CLOSE")
            {
                var cwt = self.GetCWT();

                cwt.Page.Closing = true;
                cwt.Page.TargetAlpha = 0f;
            }
        }

        private static bool FilterDialog_GetChecked(On.Menu.FilterDialog.orig_GetChecked orig, FilterDialog self, CheckBox box)
        {
            //Do not return orig
            return true;
        }

        private static void FilterDialog_SetChecked(On.Menu.FilterDialog.orig_SetChecked orig, FilterDialog self, CheckBox box, bool c)
        {

        }
    }
}
