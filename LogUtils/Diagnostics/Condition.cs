using LogUtils.Diagnostics.Tests;
using System;
using System.Collections.Generic;

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
        /// The pass/fail state of the condition
        /// </summary>
        internal Condition.Result Result;

        public readonly bool Passed => Result.Passed;

        /// <summary>
        /// Indicates whether more processing is necessary to produce a condition result
        /// </summary>
        public readonly bool ShouldProcess => Passed;

        public Condition()
        {
            Result = new Condition.Result();
        }

        public Condition(T value, IConditionHandler handler) : this()
        {
            Value = value;

            if (handler != null)
                AddHandler(handler);
        }

        public void AddHandler(IConditionHandler handler)
        {
            Result.Handlers.Add(handler);
        }

        public void AddHandlers(IEnumerable<IConditionHandler> handlers)
        {
            Result.Handlers.AddRange(handlers);
        }

        /// <summary>
        /// Indicates that the result should expicitly indicate the expected, or unexpected state, assuming passing as the expected state
        /// </summary>
        /// <remarks>Only effective when you use a handler that defers result processing such as DeferredAssertHandler</remarks>
        public void ExpectPass()
        {
            Result.Expectation = Condition.State.Pass;
        }

        /// <summary>
        /// Indicates that the result should expicitly indicate the expected, or unexpected state, assuming failing as the expected state
        /// </summary>
        /// <remarks>Only effective when you use a handler that defers result processing such as DeferredAssertHandler</remarks>
        public void ExpectFail()
        {
            Result.Expectation = Condition.State.Fail;
        }

        public void Pass()
        {
            Result.Passed = true;
            Result.Handle();
        }

        public void Fail(Condition.Message reportMessage)
        {
            Result.Passed = false;
            Result.Message = reportMessage;
            Result.Handle();
        }
    }

    public static class Condition
    {
        /// <summary>
        /// Asserts that the input value is true
        /// </summary>
        /// <remarks>Default handle behavior is to log a message when the asserted value is not true</remarks>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static Condition<bool> Assert(bool condition)
        {
            return Diagnostics.Assert.That(condition).IsTrue();
        }

        /// <inheritdoc cref="Assert(bool)"/>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        public static Condition<bool> Assert(bool condition, AssertBehavior behavior)
        {
            return Diagnostics.Assert.That(condition, behavior).IsTrue();
        }

        /// <inheritdoc cref="Assert(bool)"/>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        public static Condition<bool> Assert(bool condition, IConditionHandler handler)
        {
            return Diagnostics.Assert.That(condition, handler).IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is false
        /// </summary>
        /// <remarks>Default handle behavior is to log a message when the asserted value is true</remarks>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static Condition<bool> AssertFalse(bool condition)
        {
            return Diagnostics.Assert.That(condition).IsFalse();
        }

        /// <inheritdoc cref="AssertFalse(bool)"/>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="behavior">The expected behavior of the assert</param>
        public static Condition<bool> AssertFalse(bool condition, AssertBehavior behavior)
        {
            return Diagnostics.Assert.That(condition, behavior).IsFalse();
        }

        /// <inheritdoc cref="AssertFalse(bool)"/>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
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
            /// A set of strings to be appended to the end of the message for result reports
            /// </summary>
            public HashSet<string> Tags = new HashSet<string>();

            /// <summary>
            /// Constructs a response message
            /// </summary>
            /// <param name="message">The raw unformatted message string</param>
            public Message(string message) : base()
            {
                Raw = message;
                Descriptors = Array.Empty<string>();
            }

            /// <inheritdoc cref="Message(string)"/>
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

        public class Result
        {
            private static int _nextID;

            /// <summary>
            /// Value to be assigned to the next created Result instance
            /// </summary>
            private static int nextID
            {
                get
                {
                    //Maintaining a count for every result could easily balloon into a very high value - limit counting to only test results
                    if (TestSuite.ActiveSuite == null)
                        return 0;

                    _nextID++;
                    return _nextID;
                }
            }

            /// <summary>
            /// Value used for identification of the result (non-zero based)
            /// </summary>
            public int ID = nextID;

            public bool Passed;
            public Message Message;

            public bool HasEmptyMessage => string.IsNullOrEmpty(ToString());

            public bool HasExpectation => Expectation != State.None;

            /// <summary>
            /// Checks that there is an expected outcome and the result is consistent with that outcome
            /// </summary>
            public bool IsUnexpected
            {
                get
                {
                    //This property only cares about compating against an expected outcome
                    if (!HasExpectation)
                        return false;

                    State expectedResult = Expectation;

                    if (Passed)
                        return expectedResult != State.Pass;
                    else
                        return expectedResult != State.Fail;
                }
            }

            /// <summary>
            /// Optional field that can be used to change how a result outcome is interpreted by comparing it to an expected outcome (e.g. fail may not always be treated as a fail)
            /// </summary>
            public State Expectation;

            /// <summary>
            /// The handlers responsible for handling the assertion result
            /// </summary>
            public List<IConditionHandler> Handlers;

            public Result()
            {
                Passed = true;
                Expectation = State.None;
                Handlers = new List<IConditionHandler>();
                Message = Message.Empty;
            }

            /// <summary>
            /// Compiles a set of supported tags for the purpose of appending to a condition response message
            /// </summary>
            public void CompileMessageTags()
            {
                if (HasExpectation)
                {
                    string expectationTag = IsUnexpected ? UtilityConsts.MessageTag.UNEXPECTED : UtilityConsts.MessageTag.EXPECTED;
                    Message.Tags.Add(expectationTag);
                }

                if (HasEmptyMessage)
                    Message.Tags.Add(UtilityConsts.MessageTag.EMPTY);
            }

            public void Handle()
            {
                foreach (var handler in Handlers)
                    handler.Handle(this);
            }

            /// <summary>
            /// Checks that a result is consistent with a set expectation, or if it has passed when none is set
            /// </summary>
            public bool PassedWithExpectations()
            {
                if (!HasExpectation)
                    return Passed;

                return !IsUnexpected;
            }

            /// <summary>
            /// Gets the result string
            /// </summary>
            public override string ToString()
            {
                string resultMessage = Message.ToString();

                if (resultMessage != null)
                    return resultMessage;
                return string.Empty;
            }

            /// <summary>
            /// Change the next result ID to zero
            /// </summary>
            public static void ResetCount()
            {
                _nextID = 0;
            }
        }

        public enum State
        {
            None,
            Pass,
            Fail
        }
    }
}
