using BepInEx.Logging;
using System;

namespace LogUtils.CompatibilityServices
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
