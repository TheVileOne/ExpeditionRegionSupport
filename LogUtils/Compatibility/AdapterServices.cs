using BepInEx.Logging;
using LogUtils.Compatibility.BepInEx;
using System;

namespace LogUtils.Compatibility
{
    public static class AdapterServices
    {
        /// <summary>
        /// Creates a wrapper object that can interface with a ManualLogSource
        /// </summary>
        /// <exception cref="ArgumentNullException">Provided source is null</exception>
        public static IExtendedLogSource Convert(ManualLogSource source)
        {
            return ManualLogSourceWrapper.FromSource(source);
        }
    }
}
