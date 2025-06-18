using LogUtils.Compatibility.BepInEx;
using System;

namespace LogUtils.Helpers
{
    public static class ComponentUtils
    {
        public static UtilityComponent FindWithTag(string tag)
        {
            return Array.Find(BepInExInfo.ManagerObject.GetComponents<UtilityComponent>(), c => c.Tag == tag);
        }

        public static T GetOrCreate<T>(string tag, out bool didCreate) where T : UtilityComponent
        {
            didCreate = false;
            T managedComponent = managedComponent = BepInExInfo.ManagerObject.GetComponent<T>();

            if (managedComponent == null)
            {
                didCreate = true;
                managedComponent = BepInExInfo.ManagerObject.AddComponent<T>();
            }
            return managedComponent;
        }
    }
}
