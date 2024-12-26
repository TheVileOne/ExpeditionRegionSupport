using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public static class Condition
    {
        public static readonly List<IConditionHandler> AssertHandlers = new List<IConditionHandler>();

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition)
        {
            var assert = new Condition<bool>(condition, AssertHandler.DefaultHandler);
            return assert.IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition, AssertBehavior behavior)
        {
            var assert = new Condition<bool>(condition, AssertHandler.DefaultHandler.Clone(behavior));
            return assert.IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition, IConditionHandler handler)
        {
            var assert = new Condition<bool>(condition, handler);
            return assert.IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition)
        {
            var assert = new Condition<bool>(condition, AssertHandler.DefaultHandler);
            return assert.IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition, AssertBehavior behavior)
        {
            var assert = new Condition<bool>(condition, AssertHandler.DefaultHandler.Clone(behavior));
            return assert.IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition, IConditionHandler handler)
        {
            var assert = new Condition<bool>(condition, handler);
            return assert.IsFalse();
        }

        public class Message
        {
            public static Message Empty => new Message(string.Empty);

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

        public struct Result
        {
            public bool Passed;
            public Message Message;
        }
    }
}
