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

        /// <summary>
        /// The utility uses data from this class when mod-specific data is unavailable
        /// </summary>
        public static ModData Default;

        static ModData()
        {
            Default = new ModData();
            Default.AssertTargets.Add(LogID.Unity);
        }

        public static bool HasDataEntries => DataDictionary.Count > 0;

        public static bool HasAssertTargets
        {
            get
            {
                if (!HasDataEntries) return false;

                var enumerator = DataDictionary.Values.GetEnumerator();

                bool targetsFound = false;
                while (!targetsFound && enumerator.MoveNext())
                {
                    ModData data = enumerator.Current;

                    if (data != null)
                        targetsFound = data.AssertTargets.Count > 0;
                }
                return targetsFound;
            }
        }

        public static ModData Get(BaseUnityPlugin plugin)
        {
            string dataKey = plugin.Info.Metadata.GUID;
            ModData data = DataDictionary[dataKey];

            //We shouldn't keep empty data instances registered
            if (data == null)
            {
                DataDictionary.Remove(dataKey);
                throw new KeyNotFoundException();
            }
            return data;
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
