using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public interface ILogger
    {
        public void LogInfo(object data);
        public void LogMessage(object data);
        public void LogDebug(object data);
        public void LogWarning(object data);
        public void LogError(object data);
    }
}
