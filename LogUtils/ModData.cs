using BepInEx;
using LogUtils.Enums;
using System.Collections.Generic;

namespace LogUtils
{
    public class ModData
    {
        public List<LogID> AssertTargets = new List<LogID>();

        #region Static members
        /// <summary>
        /// Contains mod-specific utility fields and settings. Any mod that intends to override features that support it should do so through a ModData instance added here
        /// <br>Uses plugin ID as a key</br>
        /// </summary>
        public static Dictionary<string, ModData> DataDictionary = new Dictionary<string, ModData>();

        public static ModData Get(BaseUnityPlugin plugin)
        {
            return DataDictionary[plugin.Info.Metadata.GUID];
        }

        public static bool TryGet(BaseUnityPlugin plugin, out ModData data)
        {
            try
            {
                data = Get(plugin);
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        public static void Set(BaseUnityPlugin plugin, ModData data)
        {
            DataDictionary[plugin.Info.Metadata.GUID] = data;
        }
        #endregion
    }
}
