using System;
using System.Diagnostics;

namespace LogUtils.Timers
{
    public sealed class TimerScope : IDisposable
    {
        private readonly Stopwatch _timer;
        private readonly ILogger _logger;

        private long elapsedTicksOnStartup;

        internal TimerScope(Stopwatch timer, ILogger logger)
        {
            _timer = timer;
            _logger = logger;
            elapsedTicksOnStartup = _timer.ElapsedTicks;
        }

        /// <summary>
        /// Reports the time in milliseconds that the scoped region took to execute
        /// </summary>
        public void Dispose()
        {
            long elapsedTicksSinceStartup = _timer.ElapsedTicks - elapsedTicksOnStartup;

            TimeSpan timeTaken = TimeSpan.FromTicks(elapsedTicksSinceStartup);
            _logger.LogDebug("Scope execution time: " + (int)timeTaken.TotalMilliseconds + " ms");
            _timer.Stop();
        }
    }
}
