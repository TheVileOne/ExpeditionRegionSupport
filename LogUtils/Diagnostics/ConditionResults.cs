using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public struct ConditionResults
    {
        /// <summary>
        /// When applicable, this contains descriptor terms used to format result messages
        /// </summary>
        public List<string> Descriptors = new List<string>();
        public string FailMessage, PassMessage;
        public readonly ConditionStatus Status;

        public readonly bool Failed => Status == ConditionStatus.Fail;
        public readonly bool Passed => Status == ConditionStatus.Pass;

        public ConditionResults(bool conditionPassed)
        {
            Status = conditionPassed ? ConditionStatus.Pass : ConditionStatus.Fail;
        }

        /// <summary>
        /// Clear descriptors and replace with a new set of desciptors
        /// </summary>
        public void SetDescriptors(params string[] descriptors)
        {
            Descriptors.Clear();
            Descriptors.AddRange(descriptors);
        }

        public override string ToString()
        {
            string reportMessage = Passed ? PassMessage : FailMessage;

            if (reportMessage != null && Descriptors.Count > 0)
            {
                try
                {
                    reportMessage = string.Format(reportMessage, Descriptors.ToArray());
                }
                catch (FormatException)
                {
                }
            }

            return reportMessage;
        }
    }

    public enum ConditionStatus
    {
        Pass,
        Fail
    }
}
