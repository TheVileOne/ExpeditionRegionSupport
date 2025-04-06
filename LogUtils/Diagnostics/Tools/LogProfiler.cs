using System;
using System.Diagnostics;

namespace LogUtils.Diagnostics.Tools
{
    public class LogProfiler
    {
        public const int SAMPLE_FREQUENCY = 10;

        /// <summary>
        /// The average window of time in between write requests
        /// </summary>
        public TimeSpan AverageLogRate = TimeSpan.Zero;

        public Predicate<LogProfiler> BufferConditions = defaultBufferConditions;

        public bool ShouldUseBuffer => BufferConditions.Invoke(this);

        public int BufferedFrameCount;

        public TimeSpan LastSamplingTime = TimeSpan.Zero;

        private int _messagesSinceLastSampling;

        public int MessagesSinceLastSampling
        {
            get => _messagesSinceLastSampling;
            set
            {
                if (!canUpdate) return;
                _messagesSinceLastSampling = value;
            }
        }

        /// <summary>
        /// The polling frequency represented as the number of samples required to collect a new sample 
        /// </summary>
        public int SampleFrequency = SAMPLE_FREQUENCY;

        public bool IsReadyToAnalyze => canUpdate && MessagesSinceLastSampling >= SampleFrequency;

        /// <summary>
        /// The minimum rate between received messages considered to be normal, (or perhaps above normal, but not high volume). The value 2.5f represents 1/10th the length of
        /// a typical RainWorld frame at 40 FPS (25 ms / 10). Values less than this value are considered to be high volume.
        /// </summary>
        public float LogRateThreshold = 2.5f;

        /// <summary>
        /// The amount of consecutive sampling periods exceeding the allowable average rate (of logged messages) allowed before triggering the message buffer
        /// </summary>
        public ushort HighVolumeSustainmentThreshold = 1;

        /// <summary>
        /// The current amount of consecutive sampling periods exceeding the allowable average rate (of logged messages)
        /// </summary>
        public ushort PeriodsUnderHighVolume;

        /// <summary>
        /// Average is multiplied by this amount for each period there are no new messages to sample
        /// </summary>
        public float LogRateDecay = 0.25f;

        public bool IsEnabled => canUpdate;

        private bool canUpdate;

        public void Start()
        {
            if (canUpdate) return;

            canUpdate = true;
            LastSamplingTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
        }

        public void Stop()
        {
            canUpdate = false;

            if (BufferedFrameCount > 0)
                UtilityLogger.DebugLog($"Buffered {BufferedFrameCount} messages");

            BufferedFrameCount = 0;
            PeriodsUnderHighVolume = 0;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void UpdateCalculations()
        {
            if (!canUpdate) return;

            int accumulatedMessageCount = MessagesSinceLastSampling;

            TimeSpan currentTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());

            AverageLogRate = CalculateLogAverage(LastSamplingTime, currentTime, accumulatedMessageCount);

            MessagesSinceLastSampling = 0;
            LastSamplingTime = currentTime;
        }

        protected TimeSpan CalculateLogAverage(TimeSpan startTime, TimeSpan endTime, int messageCount)
        {
            //LogAverage should not stay the same after multiple periods of zero activity
            if (messageCount == 0)
                return AverageLogRate + new TimeSpan((long)(AverageLogRate.Ticks * LogRateDecay));

            TimeSpan timeElapsed = endTime - startTime;
            return TimeSpan.FromTicks(timeElapsed.Ticks / messageCount);
        }

        private static bool defaultBufferConditions(LogProfiler profiler)
        {
            return profiler.canUpdate && profiler.PeriodsUnderHighVolume >= profiler.HighVolumeSustainmentThreshold;
        }
    }
}
