﻿using System;
using UnityEngine;

namespace LogUtils.Helpers
{
    public static class ComponentUtils
    {
        /// <summary>
        /// The GameObject that contains the game, and all modded assemblies
        /// </summary>
        public static GameObject ManagerObject => BepInEx.Bootstrap.Chainloader.ManagerObject;

        public static UtilityComponent FindWithTag(string tag)
        {
            return Array.Find(ManagerObject.GetComponents<UtilityComponent>(), c => c.Tag == tag);
        }

        public static T GetOrCreate<T>(string tag, out bool didCreate) where T : UtilityComponent
        {
            didCreate = false;
            T managedComponent = null;

            Version activeVersion = null;
            try
            {
                managedComponent = ManagerObject.GetComponent<T>();

                if (managedComponent == null)
                {
                    didCreate = true;
                    managedComponent = ManagerObject.AddComponent<T>();
                }

                activeVersion = managedComponent.Version;
                return managedComponent;
            }
            catch (TypeLoadException) //There was some kind of version mismatch
            {
                UtilityComponent component = FindWithTag(tag);
                activeVersion = component.Version;
            }
            finally
            {
                if (managedComponent == null) //Loading the component failed for some reason
                    UtilityLogger.LogWarning("Utility component failed to load");

                //TODO: Replace UtilityComponent with most up to data version
                if (activeVersion != null && activeVersion < UtilityCore.AssemblyVersion)
                    UtilityLogger.LogWarning("Utility component version out of date");
            }
            return null;
        }
    }
}
