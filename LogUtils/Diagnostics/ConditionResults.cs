using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public struct ConditionResults
    {
        public static ConditionResults Fail => new ConditionResults(false);
        public static ConditionResults Pass => new ConditionResults(true);

        /// <summary>
        /// The response message object created from processing the assert conditions 
        /// </summary>
        public Message Response;

        public readonly ConditionStatus Status;

        public readonly bool Failed => Status == ConditionStatus.Fail;
        public readonly bool Passed => Status == ConditionStatus.Pass;

        public ConditionResults(bool conditionPassed)
        {
            Status = conditionPassed ? ConditionStatus.Pass : ConditionStatus.Fail;
        }

        public override string ToString()
        {
            return Response?.ToString();
        }

        public class Message
        {
            /// <summary>
            /// When applicable, this contains descriptor terms used to format result messages
            /// </summary>
            public string[] Descriptors;

            /// <summary>
            /// Message with no formatting applied
            /// </summary>
            public string Raw;

            /// <summary>
            /// Constructs a response message
            /// </summary>
            /// <param name="message">The raw unformatted message string</param>
            public Message(string message) : base()
            {
                Raw = message;
                Descriptors = Array.Empty<string>();
            }

            /// <summary>
            /// Constructs a response message
            /// </summary>
            /// <param name="message">The raw unformatted message string</param>
            /// <param name="formatValues">Values to use for formatting the raw string</param>
            public Message(string message, params string[] formatValues)
            {
                Raw = message;
                Descriptors = formatValues;
            }

            /// <summary>
            /// Replace the current format arguments with a new set of arguments
            /// </summary>
            /// <param name="descriptors">The new format arguments</param>
            /// <param name="throwIfDescriptorCountDoesNotMatch">A flag to remind mod users to update the raw string, before changing the number of format arguments</param>
            /// <exception cref="ArgumentException">The argument provided has an improper length</exception>
            public void SetDescriptors(string[] descriptors, bool throwIfDescriptorCountDoesNotMatch = true)
            {
                //To use the API properly, mod users need to acknowledge that changing the amount of format arguments is intended and allowable
                if (throwIfDescriptorCountDoesNotMatch && descriptors.Length != Descriptors.Length)
                    throw new ArgumentException("Changing the descriptor count is not allowed");
                Descriptors = descriptors;
            }

            public override string ToString()
            {
                if (Raw == null || Descriptors.Length == 0)
                    return Raw;

                try
                {
                    return string.Format(Raw, Descriptors);
                }
                catch (FormatException)
                {
                    return "Unable to format response";
                }
            }
        }
    }

    public enum ConditionStatus
    {
        Pass,
        Fail
    }
}
