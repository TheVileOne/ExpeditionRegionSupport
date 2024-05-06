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

        public string TotalTimeHeader = "Entire process";

        public TimerResults TotalTimeReported => new TimerResults(TotalTimeHeader, Results.ElapsedTime);

        public bool ReportTotalTime;
        public TimerOutput OutputFormat;

        /// <summary>
        /// Create an instance of a MultiUseTimer
        /// </summary>
        /// <param name="outputFormat">Determines how results are output as a string</param>
        /// <param name="allowResultLogging">Should results log immediately to file</param>
        public MultiUseTimer(TimerOutput outputFormat, bool allowResultLogging) : base(allowResultLogging)
        {
            OutputFormat = outputFormat;
            ReportTotalTime = OutputFormat != TimerOutput.AbsoluteTime;
        }

        /// <summary>
        /// Create an instance of a MultiUseTimer
        /// </summary>
        /// <param name="allowResultLogging">Should results log immediately to file</param>
        public MultiUseTimer(bool allowResultLogging) : this(TimerOutput.AbsoluteTime, allowResultLogging)
        {
        }

        public override void ReportTime(string reportHeader = "Process")
        {
            bool wasLoggingAllowed = AllowResultLogging;

            if (AllResults.Count > 0 && OutputFormat == TimerOutput.RelativeIncrements)
                AllowResultLogging = false; //Base method will log the wrong value to file

            base.ReportTime(reportHeader);

            AllowResultLogging = wasLoggingAllowed;

            if (AllowResultLogging && AllResults.Count > 0 && OutputFormat == TimerOutput.RelativeIncrements)
            {
                TimerResults lastResult = AllResults.Last();
                TimerResults relativeResult = ConvertToRelative(Results, lastResult);
                Plugin.Logger.LogDebug(relativeResult);
            }

            AllResults.Add(Results);
        }

        public override string ToString()
        {
            if (AllResults.Count == 0)
                return "No time data available";

            StringBuilder sb = new StringBuilder();

            TimerResults lastResults = new TimerResults();
            foreach (TimerResults results in AllResults)
            {
                if (OutputFormat == TimerOutput.RelativeIncrements)
                {
                    TimerResults relativeResult = ConvertToRelative(results, lastResults);
                    sb.AppendLine(relativeResult.ToString());
                }
                else
                    sb.AppendLine(results.ToString());
                lastResults = results;
            }

            if (ReportTotalTime)
                sb.AppendLine(TotalTimeReported.ToString());

            return sb.ToString();
        }

        public TimerResults ConvertToRelative(TimerResults laterResult, TimerResults earlierResult, string header = null)
        {
            return new TimerResults(header ?? laterResult.Header, laterResult.ElapsedTime - earlierResult.ElapsedTime);
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

    public enum TimerOutput
    {
        AbsoluteTime, //The time is formatted to string as reported in the TimerResults
        RelativeIncrements //The time is formatted to string as the increment difference from the last reported TimerResults, absolute timing for first result
    }
}
