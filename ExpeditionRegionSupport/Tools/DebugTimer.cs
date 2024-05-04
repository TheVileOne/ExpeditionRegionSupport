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

        /// <summary>
        /// When set to true, the results will be logged to file on report
        /// </summary>
        public bool AllowResultLogging;

        public TimerResults Results { get; protected set; }

        public DebugTimer(bool allowResultLogging = true)
        {
            AllowResultLogging = allowResultLogging;
        }

        public virtual void ReportTime(string reportHeader = "Process")
        {
            Results = new TimerResults(reportHeader, timer.ElapsedMilliseconds);

            if (AllowResultLogging)
                Plugin.Logger.LogDebug(Results);
        }

        public void Reset() => timer.Reset();
        public void Restart() => timer.Restart();
        public void Start() => timer.Start();
        public void Stop() => timer.Stop();

        public override string ToString()
        {
            return Results.ToString();
        }
    }

    public class MultiUseTimer : DebugTimer
    {
        public HashSet<TimerResults> AllResults = new HashSet<TimerResults>(); 

        public MultiUseTimer(bool allowResultLogging) : base(allowResultLogging)
        {
        }

        public override void ReportTime(string reportHeader = "Process")
        {
            base.ReportTime(reportHeader);
            AllResults.Add(Results);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (TimerResults results in AllResults)
                sb.AppendLine(results.ToString());
            return sb.ToString();
        }
    }

    public readonly struct TimerResults
    {
        public string Header { get; }
        public long ElapsedTime { get; }

        public TimerResults(string header, long elapsedTime)
        {
            Header = header;
            ElapsedTime = elapsedTime;
        }

        public override string ToString()
        {
            if (Equals(default))
                return "No time recorded";

            return $"{Header} took {ElapsedTime} ms";
        }
    }
}
