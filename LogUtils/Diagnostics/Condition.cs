using System;

namespace LogUtils.Diagnostics
{
    public struct Condition<T>
    {
        public static bool operator true(Condition<T> condition) => condition.Passed;
        public static bool operator false(Condition<T> condition) => !condition.Passed;

        /// <summary>
        /// Contains the state of the condition to evaluate
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// The handler responsible for handling the assertion result
        /// </summary>
        public IConditionHandler Handler;

        /// <summary>
        /// The pass/fail state of the condition
        /// </summary>
        internal Condition.Result Result;

        public readonly bool Passed => Result.Passed;

        public readonly bool ShouldProcess => Passed;

        public Condition(T value, IConditionHandler handler)
        {
            Value = value;
            Handler = handler;
            Result.Passed = true;
        }

        public void Pass()
        {
            Result.Passed = true;
            onResult();
        }

        public void Fail(Condition.Message reportMessage)
        {
            Result.Passed = false;
            Result.Message = reportMessage;
            onResult();
        }

        private void onResult()
        {
            Handler?.Handle(in this);
        }
    }

    public static class Condition
    {
        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static Condition<bool> Assert(bool condition)
        {
            return Diagnostics.Assert.That(condition).IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static Condition<bool> Assert(bool condition, AssertBehavior behavior)
        {
            return Diagnostics.Assert.That(condition, behavior).IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static Condition<bool> Assert(bool condition, IConditionHandler handler)
        {
            return Diagnostics.Assert.That(condition, handler).IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static Condition<bool> AssertFalse(bool condition)
        {
            return Diagnostics.Assert.That(condition).IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static Condition<bool> AssertFalse(bool condition, AssertBehavior behavior)
        {
            return Diagnostics.Assert.That(condition, behavior).IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static Condition<bool> AssertFalse(bool condition, IConditionHandler handler)
        {
            return Diagnostics.Assert.That(condition, handler).IsFalse();
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
