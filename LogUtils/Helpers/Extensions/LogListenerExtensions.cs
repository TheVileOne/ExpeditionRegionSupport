using BepInEx.Logging;
using System;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        internal static void TryDispose(this ILogListener listener)
        {
            if (listener == null) return;

            try
            {
                listener.Dispose(); //This will flush any messages held by the original listener
            }
            catch
            {
                //BepInEx library doesn't handle disposal very safely
            }
            finally
            {
                GC.SuppressFinalize(listener); //Suppress since this version of BepInEx doesn't suppress on dispose
            }
        }
    }
}
