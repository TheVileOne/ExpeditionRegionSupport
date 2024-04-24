using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Tools
{
    public class DebugTimer
    {
        private Stopwatch timer = new Stopwatch();

        public void ReportTime(string reportHeader = "Process")
        {
            Plugin.Logger.LogDebug($"{reportHeader} took {timer.ElapsedMilliseconds} ms");
        }

        public void Reset() => timer.Reset();
        public void Restart() => timer.Restart();
        public void Start() => timer.Start();
        public void Stop() => timer.Stop();
    }
}
