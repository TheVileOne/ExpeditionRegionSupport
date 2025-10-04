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

            byte[] byteStream = getResourceBytes();

            Version resourceVersion = Assembly.ReflectionOnlyLoad(byteStream).GetName().Version;
            UtilityLogger.Log("Patcher resource version: " + resourceVersion);

            string patcherFilename = "LogUtils.VersionLoader.dll";
            string patcherPath = Path.Combine(BepInExPath.PatcherPath, patcherFilename);

            //These are used as a workaround solution that ensures the VersionLoader can be updated without intermittently losing patcher functionality during the transition
            string transferFilename = "LogUtils.VersionLoader (Patch).dll";
            string transferPath = Path.Combine(BepInExPath.PatcherPath, transferFilename);

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
                            replaceWithNewVersion();
                        else
                        {
                            UtilityEvents.OnSetupPeriodReached += replaceWithNewVersionEvent;

                            void replaceWithNewVersionEvent(SetupPeriodEventArgs e)
                            {
                                if (e.CurrentPeriod < SetupPeriod.PreMods)
                                    return;

                                replaceWithNewVersion();
                                UtilityEvents.OnSetupPeriodReached -= replaceWithNewVersionEvent;
                            }
                        }
                    }
                }
                return;
            }

            if (File.Exists(transferPath))
                removeTransferFile();
            deployInternal();

            void deployInternal()
            {
                UtilityLogger.Log("Deploying patcher");
                try
                {
                    string patcherBackupPath = Path.Combine(BepInExPath.BackupPath, patcherFilename);
                    FileUtils.TryDelete(patcherBackupPath); //Patcher should never exist in both patchers, and backup directories at the same time

                    File.WriteAllBytes(patcherPath, byteStream);
                    HasDeployed = true;

                    UnityDoorstop.AddToWhitelist(patcherFilename);
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

            void replaceWithNewVersion()
            {
                //If we don't stop this process here, Rain World will black screen when the next dialog activates
                if (currentDialog != null)
                    currentProcess.StopSideProcess(currentDialog);

                currentDialog = new DialogNotify("A new version for LogUtils.VersionLoader is available.", new Vector2(450, 175), currentProcess, () =>
                {
                    UtilityLogger.Log("Replacing patcher with new version");

                    Remove();

                    //Rain World will need to be launched several times before we can replace the loaded assembly with the new version
                    patcherFilename = transferFilename;
                    patcherPath = transferPath;
                    deployInternal();

                    currentDialog = null;
                });
                currentProcess.ShowDialog(currentDialog);
            }

            void removeTransferFile()
            {
                UtilityLogger.Log("Removing patcher transfer file");
                try
                {
                    UnityDoorstop.RemoveFromWhitelist(transferFilename);
                }
                catch (FileNotFoundException)
                {
                    UtilityLogger.LogWarning("whitelist.txt is unavailable");
                }
                catch (IOException ex)
                {
                    UtilityLogger.LogError("Unable to remove file", ex);
                }
            }
        }

        private static byte[] getResourceBytes()
        {
            return (byte[])Properties.Resources.ResourceManager.GetObject(UtilityConsts.ResourceNames.PATCHER);
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
                UtilityLogger.LogError("Unable to remove file", ex);
            }
        }
    }
}
