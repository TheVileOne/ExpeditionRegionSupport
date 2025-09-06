using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.HookUtils;
using ExpeditionRegionSupport.Interface.Components;
using Extensions;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using FilterOptions = ExpeditionRegionSupport.Interface.Components.FilterOptions;
using Vector2 = UnityEngine.Vector2;

namespace ExpeditionRegionSupport.Interface
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Hooks do not need to follow case guidelines")]
    public class FilterDialogHooks
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
            On.Menu.SimpleButton.SetSize += SimpleButton_SetSize;
        }

        #region Constructor hooks

        private static bool initializePage(Dialog dialog)
        {
            bool handled = false;

            FilterDialog filterDialog = dialog as FilterDialog;

            if (filterDialog != null)
            {
                filterDialog.InitializePage();
                handled = true;
            }

            return handled;
        }

        /// <summary>
        /// This hook allows FilterDialog to use a ScrollablePage instead of a Page
        /// </summary>
        private static void Dialog_ctor_ProcessManager(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Emit dialog type check after base logic is handled
            cursor.GotoNext(MoveType.After,
                x => x.Match(OpCodes.Ldsfld),
                x => x.Match(OpCodes.Call));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(initializePage); //Initializes page when handling FilterDialogs, returns handled state

            //Branch over existing page creation logic when page has already been handled
            cursor.BranchTo(OpCodes.Brtrue, MoveType.After,
                x => x.MatchStfld<Dialog>(nameof(Dialog.dialogPage)));

            //Container assignment of darkSprite is changed from Page container to Dialog container
            cursor.GotoNext(MoveType.After, x => x.MatchLdfld<Menu.Menu>(nameof(Menu.Menu.pages)));
            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(MenuObject).GetMethod("get_Container")));
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<Menu.Menu>(OpCodes.Ldfld, nameof(Menu.Menu.container)); //Menu.container
        }

        private static void FilterDialog_ctor(On.Menu.FilterDialog.orig_ctor orig, FilterDialog self, ProcessManager manager, ChallengeSelectPage owner)
        {
            if (Plugin.DebugMode)
            {
                if (ChallengeOrganizer.filterChallengeTypes.Count == 0)
                    Plugin.Logger.LogInfo("NO ACTIVE FILTERS");
                else
                {
                    Plugin.Logger.LogInfo("ACTIVE FILTERS");
                    ChallengeOrganizer.filterChallengeTypes.ForEach(Plugin.Logger.LogInfo);
                }
            }

            var cwt = self.GetCWT();

            bool hasErrors = false;

            try
            {
                orig(self, manager, owner);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Dialog could not be loaded successfully");
                Plugin.Logger.LogError(ex);

                hasErrors = true;
            }

            cwt.InitSuccess = !hasErrors;
        }

        private static void FilterDialog_ctor(ILContext il)
        {
            FilterDialog_ctorHook(il); //IL for handling FilterDialog
            SettingsDialog_ctorHook(il); //IL for handling ExpeditionSettingsDialog
        }

        private static void SettingsDialog_ctorHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //There is a list of availableChallengeTypes that gets processed. Find an injection point to branch over that process
            cursor.GotoNext(MoveType.After, x => x.MatchStfld<FilterDialog>(nameof(FilterDialog.checkBoxes)));
            cursor.GotoNext(MoveType.After, x => x.MatchStloc(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<FilterDialog, bool>>(d => d is ExpeditionSettingsDialog);

            //Branch to cancel button selectable logic
            cursor.BranchTo(OpCodes.Brtrue, MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<FilterDialog>(nameof(FilterDialog.cancelButton)));

            //Go to before loop processing Challenge filters. ExpeditionSettingsDialog doesn't need any of this logic in the loop to run
            for (int i = 0; i < 2; i++)
            {
                cursor.GotoNext(MoveType.After, //Go past two local integers before the loop indexer
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(out _));
            }

            cursor.Emit(OpCodes.Ldarg_0);

            static bool initializeSettingsDialog(FilterDialog dialog)
            {
                ExpeditionSettingsDialog settingsDialog = dialog as ExpeditionSettingsDialog;

                if (settingsDialog != null)
                {
                    settingsDialog.InitializeCheckBoxes();
                    return true;
                }
                return false;
            }
            cursor.EmitDelegate(initializeSettingsDialog);

            //Branch over loop
            cursor.BranchTo(OpCodes.Brtrue, MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(1),
                x => x.MatchStfld<FilterDialog>(nameof(FilterDialog.opening)));

            cursor.GotoNext(MoveType.Before, x => x.MatchRet());
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(onFilterDialogCreated);
        }

        private static void FilterDialog_ctorHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            int localMenuLabelID = 0,
                localCheckBoxID = 0,
                localSpriteID = 0;

            cursor.GotoNext(MoveType.After,
                x => x.MatchLdfld<FilterDialog>(nameof(FilterDialog.challengeTypes)),
                x => x.MatchLdloc(out localMenuLabelID)); //Get local id for created MenuLabel

            //Jump to first branch over position

            cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Callvirt)); //This Callvirt adds a MenuLabel to challengeTypes

            //First subObjects branch over

            cursor.BranchTo( //Branch past second reference to label
                x => x.MatchLdloc(localMenuLabelID),
                x => x.Match(OpCodes.Callvirt));

            //Replace CheckBox

            cursor.GotoNext(MoveType.After, x => x.MatchNewobj<CheckBox>()); //Go to CheckBox instantiation
            cursor.GotoNext(MoveType.Before, x => x.MatchStloc(out localCheckBoxID)); //Get local id for created CheckBox
            cursor.Emit(OpCodes.Ldloc, localMenuLabelID);
            cursor.EmitDelegate(replaceCheckBox); //Takes a CheckBox, and MenuLabel as arguments

            //Second subObjects branch over

            cursor.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.Menu>(nameof(Menu.Menu.pages)));

            cursor.BranchTo(
                x => x.MatchLdloc(localCheckBoxID),
                x => x.Match(OpCodes.Callvirt));

            //Branch over divider handling

            cursor.GotoNext(MoveType.After,
                x => x.MatchNewobj<FSprite>(), //Go to FSprite instantiation
                x => x.MatchStloc(out localSpriteID)); //Get local id for created FSprite

            cursor.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Menu.Menu>(nameof(Menu.Menu.container)));

            cursor.BranchTo(
                x => x.MatchLdfld<FilterDialog>(nameof(FilterDialog.dividers)),
                x => x.MatchLdloc(localSpriteID),
                x => x.Match(OpCodes.Callvirt));
        }

        /// <summary>
        /// This hook isolates the sprite container for a checkbox belonging to a FilterDialog. 
        /// This makes it easier to retrieve any transfer CheckBox sprites into the FilterCheckBox class
        /// </summary>
        private static void CheckBox_ctor(On.Menu.CheckBox.orig_ctor orig, CheckBox self, Menu.Menu menu, MenuObject owner, CheckBox.IOwnCheckBox reportTo, Vector2 pos, float textWidth, string displayText, string IDString, bool textOnRight)
        {
            try
            {
                self.menu = menu;
                self.owner = owner;

                if (menu is FilterDialog && self is not FilterCheckBox)
                    self.Container = new FContainer();

                orig(self, menu, owner, reportTo, pos, textWidth, displayText, IDString, textOnRight);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static FilterCheckBox replaceCheckBox(CheckBox box, MenuLabel label)
        {
            try
            {
                FilterDialog dialog = (FilterDialog)box.menu;
                FilterOptions filters = dialog.GetCWT().Options;
                FilterCheckBox filterBox = new FilterCheckBox(dialog, filters, null, box.pos, label, box.IDString);

                //Do some cleanup of stuff that was handled in CheckBox constructor
                box.owner.RecursiveRemoveSelectables(box);

                filters.AddOption(filterBox);

                return filterBox;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }

            return null;
        }

        public static void onFilterDialogCreated(FilterDialog dialog)
        {
            try
            {
                var cwt = dialog.GetCWT();

                dialog.pageTitle.x = 683f; //720f doesn't work anymore, and I'm not sure why it worked in the first place
                dialog.container.AddChild(cwt.Page.Container);

                //Replace the reference, so that mods will add to the new reference instead
                dialog.dividers = cwt.Options.Dividers;
                dialog.challengeTypes = cwt.Options.Filters;

                dialog.OpenFilterDialog();

                if (dialog is not ExpeditionSettingsDialog)
                {
                    cwt.Options.OnFilterChanged -= onFilterChanged;

                    //Find applicable filters, and update checkbox states
                    foreach (string challengeType in ChallengeOrganizer.filterChallengeTypes)
                    {
                        CheckBox box = null;
                        if (ChallengeOrganizer.availableChallengeTypes.Exists(ch => ch.GetTypeName() == challengeType))
                            (box = dialog.checkBoxes.Find(ch => ch.IDString == challengeType)).Checked = false;

                        /*
                        if (box != null)
                        {
                            Plugin.Logger.LogDebug("Base " + box.Checked);
                            Plugin.Logger.LogDebug("Custom " + (box as FilterCheckBox).Checked);
                            box = null;
                        }
                        */
                    }

                    cwt.Options.OnFilterChanged += onFilterChanged;

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

                            //TODO:Check behavior
                            //Index position should be the same for all lists
                            foreach (int filterIndex in filterIndexes)
                            {
                                CheckBox filterBox = dialog.checkBoxes[filterIndex];
                                MenuLabel filterLabel = dialog.challengeTypes[filterIndex];

                                FilterCheckBox filterOption = replaceCheckBox(filterBox, filterLabel);
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static void onFilterChanged(FilterCheckBox box, bool checkState)
        {
            bool filterApplied = !checkState; //Checked means option is not filtered. A filter is applied when option is unchecked.

            Plugin.Logger.LogDebug("Filter set: " + box.label.text + " " + checkState);

            if (filterApplied && !ChallengeOrganizer.filterChallengeTypes.Contains(box.IDString))
            {
                ExpLog.Log("Add " + box.IDString);
                ChallengeOrganizer.filterChallengeTypes.Add(box.IDString);
            }
            else if (!filterApplied)
            {
                ExpLog.Log("Remove " + box.IDString);
                ChallengeOrganizer.filterChallengeTypes.Remove(box.IDString);
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

            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Menu.Menu>(nameof(Menu.Menu.Update)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(updateHook); //Logic here cannot be handled with vanilla logic anymore. Use custom handling

            cursor.BranchTo(OpCodes.Br, MoveType.Before, //Branch to cancel button handling
                x => x.Match(OpCodes.Ldarg_0),
                x => x.MatchLdfld<FilterDialog>(nameof(FilterDialog.cancelButton)));
        }

        private static void updateHook(FilterDialog dialog)
        {
            var cwt = dialog.GetCWT();

            if (cwt.Page.HasClosed)
            {
                dialog.CloseFilterDialog(true);

                cwt.Options.OnFilterChanged -= onFilterChanged;
                cwt.Page.HasClosed = false;
            }
        }

        private static void FilterDialog_Update(On.Menu.FilterDialog.orig_Update orig, FilterDialog self)
        {
            var cwt = self.GetCWT();

            if (!cwt.InitSuccess)
            {
                self.CloseFilterDialog(true);
                return;
            }

            if (!cwt.PauseButtonHandled && RWInput.CheckPauseButton(0))
            {
                self.CloseFilterDialog();
                cwt.PauseButtonHandled = true;
            }

            //Set pre-existing fields now maintained through the main page. These are kept for compatibility reasons.
            self.AssignValuesFromPage();

            if (cwt.RunOnNextUpdate != null)
            {
                cwt.RunOnNextUpdate.Invoke(self);
                cwt.RunOnNextUpdate = null;
            }

            //self.LogValues();
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
            if (message == "CLOSE")
            {
                //self.PlaySound(SoundID.MENU_Switch_Page_Out);
                self.PlaySound(SoundID.MENU_Player_Join_Game);
            }

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

        private static void SimpleButton_SetSize(On.Menu.SimpleButton.orig_SetSize orig, SimpleButton self, Vector2 newSize)
        {
            orig(self, newSize);

            if (self.GetCWT().CenterInParent)
            {
                float horizontalCenter;

                var objectWithSize = self.owner as RectangularMenuObject;

                if (objectWithSize != null)
                    horizontalCenter = objectWithSize.size.x / 2;
                else
                    horizontalCenter = 683f; //Screen width / 2

                self.SetPosX(horizontalCenter - (self.size.x / 2f));
            }
        }
    }
}
