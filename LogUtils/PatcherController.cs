using LogUtils.Compatibility.Unity;
using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using LogUtils.Policy;
using Menu;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;

namespace LogUtils
{
    internal static class PatcherController
    {
        public static bool HasDeployed;

        private static ProcessManager currentProcess => RWInfo.RainWorld.processManager;
        private static Dialog currentDialog;

        public static void Initialize()
        {
            if (!UtilityCore.IsControllingAssembly)
                return;

            //This setting takes priority over the permission setting
            if (PatcherPolicy.ShouldDeploy)
            {
                Deploy();
                return;
            }

            //Permission was already denied - don't ask again
            if (PatcherPolicy.HasAskedForPermission)
            {
                Remove();
                return;
            }

            if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.PreMods) //Late enough into init process to not have to schedule
                AskForPermission();
            else
            {
                UtilityEvents.OnSetupPeriodReached += askForPermissionEvent;

                static void askForPermissionEvent(SetupPeriodEventArgs e)
                {
                    if (e.CurrentPeriod < SetupPeriod.PreMods)
                        return;

                    AskForPermission();
                    UtilityEvents.OnSetupPeriodReached -= askForPermissionEvent;
                }
            }
        }

        public static void AskForPermission()
        {
            currentDialog = new DialogConfirm(
                    "LogUtils wants to deploy a version loader.\n\n" +
                    "This loader ensures that the most up to date version of LogUtils available will be loaded.\n" +
                    "Is it okay to deploy?", new Vector2(650, 175), currentProcess, PermissionGranted, PermissionDenied);
            currentProcess.ShowDialog(currentDialog);
        }

        internal static void PermissionGranted()
        {
            //If we don't stop this process here, Rain World will black screen when the next dialog activates
            currentProcess.StopSideProcess(currentDialog);
            currentDialog = null;

            OnPermissionResult(true);
            Deploy();

            float dialogWidth = HasDeployed ? 650 : 450;
            currentDialog = new DialogNotify(
                    HasDeployed ? "Version loader has successfully been deployed. It will activate the next time Rain World starts."
                                : "Version loader encountered an issue during deployment.", new Vector2(dialogWidth, 175), currentProcess, Dismiss);
            currentProcess.ShowDialog(currentDialog);
        }

        internal static void PermissionDenied()
        {
            OnPermissionResult(false);
            Remove();
            currentDialog = null;
        }

        internal static void Dismiss()
        {
            currentDialog = null;
        }

        internal static void OnPermissionResult(bool result)
        {
            PatcherPolicy.Config.HasAskedForPermission.SetValue(true, SaveOption.SaveLater);
            PatcherPolicy.Config.ShouldDeploy.SetValue(result, SaveOption.SaveLater);
        }

        public static void Deploy()
        {
            UtilityLogger.Log("Checking patcher status");

            byte[] byteStream = (byte[])Properties.Resources.ResourceManager.GetObject(UtilityConsts.ResourceNames.PATCHER);

            Version resourceVersion = Assembly.ReflectionOnlyLoad(byteStream).GetName().Version;
            UtilityLogger.Log("Patcher resource version: " + resourceVersion);

            string patcherPath = Path.Combine(BepInExPath.PatcherPath, "LogUtils.VersionLoader.dll");
            string patcherBackupPath = Path.Combine(BepInExPath.BackupPath, "LogUtils.VersionLoader.dll");

            if (File.Exists(patcherPath)) //Already deployed
            {
                Version activeVersion = AssemblyName.GetAssemblyName(patcherPath).Version;

                if (activeVersion == resourceVersion)
                {
                    UtilityLogger.Log("Patcher found");
                    HasDeployed = true;
                }
                else
                {
                    UtilityLogger.Log($"Current patcher version doesn't match resource - (Current {activeVersion})");
                    if (activeVersion < resourceVersion)
                    {
                        if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.PreMods) //Late enough into init process to not have to schedule
                            ReplaceWithNewVersion();
                        else
                        {
                            UtilityEvents.OnSetupPeriodReached += replaceWithNewVersionEvent;

                            static void replaceWithNewVersionEvent(SetupPeriodEventArgs e)
                            {
                                if (e.CurrentPeriod < SetupPeriod.PreMods)
                                    return;

                                ReplaceWithNewVersion();
                                UtilityEvents.OnSetupPeriodReached -= replaceWithNewVersionEvent;
                            }
                        }
                    }
                }
                return;
            }

            UtilityLogger.Log("Deploying patcher");
            try
            {
                FileUtils.TryDelete(patcherBackupPath); //Patcher should never exist in both patchers, and backup directories at the same time

                File.WriteAllBytes(patcherPath, byteStream);
                HasDeployed = true;

                UnityDoorstop.AddToWhitelist("LogUtils.VersionLoader.dll");
            }
            catch (FileNotFoundException)
            {
                UtilityLogger.LogWarning("whitelist.txt is unavailable");
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("Unable to deploy patcher", ex);
            }
        }

        public static void Remove()
        {
            UtilityLogger.Log("Checking patcher status");

            string patcherPath = Path.Combine(BepInExPath.PatcherPath, "LogUtils.VersionLoader.dll");

            if (!File.Exists(patcherPath)) //Patcher not available
            {
                UtilityLogger.Log("Patcher not found");
                return;
            }

            UtilityLogger.Log("Removing patcher");
            try
            {
                UnityDoorstop.RemoveFromWhitelist("LogUtils.VersionLoader.dll");
            }
            catch (FileNotFoundException)
            {
                UtilityLogger.LogWarning("whitelist.txt is unavailable");
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("Unable to remove patcher", ex);
            }
        }

        public static void ReplaceWithNewVersion()
        {
            //If we don't stop this process here, Rain World will black screen when the next dialog activates
            if (currentDialog != null)
                currentProcess.StopSideProcess(currentDialog);

            currentDialog = new DialogNotify("A new version for LogUtils.VersionLoader is available.", new Vector2(450, 175), currentProcess, () =>
            {
                //Patcher will be replaced the next time Rain World starts
                UtilityLogger.Log($"Replacing patcher with new version");
                Remove();
                currentDialog = null;
            });
            currentProcess.ShowDialog(currentDialog);
        }
    }
}
