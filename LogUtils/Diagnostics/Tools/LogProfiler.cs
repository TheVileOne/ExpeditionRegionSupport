using System;
using System.Diagnostics;
using UnityEngine;

namespace LogUtils.Diagnostics.Tools
{
    public class LogProfiler
    {
        public const int SAMPLE_FREQUENCY = 10;

        /// <summary>
        /// Should the latest average calulation be compounded with the previous average calculation, or replace it entirely
        /// </summary>
        public bool AccumulatedAverageMode;

        /// <summary>
        /// The average window of time in between write requests
        /// </summary>
        public TimeSpan AverageLogRate = TimeSpan.Zero;

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

        /// <summary>
        /// The total number of messages represented by the average
        /// </summary>
        public int SampleSize;

        /// <summary>
        /// The minimum rate between received messages considered to be normal, (or perhaps above normal, but not high volume). The value 2.5f represents 1/10th the length of
        /// a typical RainWorld frame at 40 FPS (25 ms / 10). Values less than this value are considered to be high volume.
        /// </summary>
        public float LogRateThreshold = 2.5f;

        /// <summary>
        /// The length of the buffer period when triggered by high volume activity
        /// </summary>
        public int HighVolumeBufferDuration = 250;

        /// <summary>
        /// The amount of consecutive sampling periods exceeding the allowable average rate (of logged messages) allowed before triggering the message buffer
        /// </summary>
        public ushort HighVolumeSustainmentThreshold = 1;

        /// <summary>
        /// The current amount of consecutive sampling periods exceeding the allowable average rate (of logged messages)
        /// </summary>
        public ushort PeriodsUnderHighVolume = 0;

        /// <summary>
        /// Average is multiplied by this amount for each period there are no new messages to sample
        /// </summary>
        protected float AccumulatedAverageDecay = 0.25f;

        public int BufferedFrameCount;

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
        }

        public void UpdateCalculations()
        {
            if (!canUpdate) return;

            int accumulatedMessageCount = MessagesSinceLastSampling;

            TimeSpan currentTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());

            //When no messages were reported in a specified period, we 
            if (accumulatedMessageCount == 0)
                currentTime -= TimeSpan.FromTicks(AverageLogRate.Ticks * SampleSize);

            if (AccumulatedAverageMode)
            {
                AverageLogRate = CalculateAccumulatedLogAverage(LastSamplingTime, currentTime, accumulatedMessageCount);
                SampleSize = GetMessageTotal(accumulatedMessageCount);
            }
            else
            {
                AverageLogRate = CalculateLogAverage(LastSamplingTime, currentTime, accumulatedMessageCount);
                SampleSize = accumulatedMessageCount;
            }

            MessagesSinceLastSampling = 0;
            LastSamplingTime = currentTime;
        }

        public void UpdateCalculations(int messageCountSinceLastProfile)
        {
            if (!canUpdate) return;

            TimeSpan currentTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());

            if (AccumulatedAverageMode)
            {
                AverageLogRate = CalculateAccumulatedLogAverage(LastSamplingTime, currentTime, messageCountSinceLastProfile);
                SampleSize = GetMessageTotal(messageCountSinceLastProfile);
            }
            else
            {
                AverageLogRate = CalculateLogAverage(LastSamplingTime, currentTime, messageCountSinceLastProfile);
                SampleSize = messageCountSinceLastProfile;
            }

            LastSamplingTime = currentTime;
        }

        internal TimeSpan CalculateAccumulatedLogAverage(TimeSpan startTime, TimeSpan endTime, int messageCountSinceLastSampling)
        {
            //LogAverage should not stay the same after multiple periods of zero activity
            if (messageCountSinceLastSampling == 0)
                return AverageLogRate + new TimeSpan((long)(AverageLogRate.Ticks * AccumulatedAverageDecay));

            TimeSpan firstAverage, secondAverage;

            firstAverage = AverageLogRate;
            secondAverage = CalculateLogAverage(startTime, endTime, messageCountSinceLastSampling);

            //Empty periods of no activity should not impact the average when the first message activity is reported
            if (firstAverage == TimeSpan.Zero)
                return secondAverage;

            //Formula for combined averages
            long tickAverage = ((firstAverage.Ticks * SampleSize) + (secondAverage.Ticks * messageCountSinceLastSampling)) / GetMessageTotal(messageCountSinceLastSampling);

            return TimeSpan.FromTicks(tickAverage);
        }

        public TimeSpan CalculateLogAverage(TimeSpan startTime, TimeSpan endTime, int messageCount)
        {
            //LogAverage should not stay the same after multiple periods of zero activity
            if (messageCount == 0)
                return AverageLogRate + new TimeSpan((long)(AverageLogRate.Ticks * AccumulatedAverageDecay));

            //Zero messages means the average should also be zero
            if (messageCount == 0)
                return TimeSpan.Zero;

            TimeSpan timeElapsed = endTime - startTime;
            return TimeSpan.FromTicks(timeElapsed.Ticks / messageCount);
        }

        internal int GetMessageTotal(int newMessageCount)
        {
            return SampleSize + newMessageCount;
        }
    }
}
