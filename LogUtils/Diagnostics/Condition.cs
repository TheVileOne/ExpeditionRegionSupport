using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public Condition()
        {
            Result.Initialize();
        }

        public Condition(T value, IConditionHandler handler) : this()
        {
            Value = value;
            Handler = handler;
        }

        /// <summary>
        /// Indicates that the result should expicitly indicate the expected, or unexpected state, assuming passing as the expected state
        /// <br>Currently is only effective when you use a handler that defers result processing such as using a DeferredAssertHandler</br>
        /// </summary>
        public void ExpectPass()
        {
            Result.Expectation = Condition.State.Pass;
        }

        /// <summary>
        /// Indicates that the result should expicitly indicate the expected, or unexpected state, assuming failing as the expected state
        /// <br>Currently is only effective when you use a handler that defers result processing such as using a DeferredAssertHandler</br>
        /// </summary>
        public void ExpectFail()
        {
            Result.Expectation = Condition.State.Fail;
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
            Handler?.Handle(Result);
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

            public bool HasEmptyMessage => string.IsNullOrEmpty(ToString());

            /// <summary>
            /// Checks that there is an expected outcome and the result is consistent with that outcome
            /// </summary>
            public bool IsUnexpected
            {
                get
                {
                    //This property only cares about compating against an expected outcome
                    if (!HasExpectation())
                        return false;

                    State expectedResult = _expectation.Value;

                    if (Passed)
                        return expectedResult != State.Pass;
                    else
                        return expectedResult != State.Fail;
                }
            }

            private StrongBox<State> _expectation;

            /// <summary>
            /// Optional property that can be used to change how a result outcome is interpreted by comparing it to an expected outcome (e.g. fail may not always be treated as a fail)
            /// </summary>
            public State Expectation
            {
                get
                {
                    //Set on initialization - null check is unnecessary
                    return _expectation.Value;
                }
                set
                {
                    if (_expectation == null)
                    {
                        _expectation = new StrongBox<State>(value);
                        return;
                    }
                    _expectation.Value = value;
                }
            }

            /// <summary>
            /// Compiles a set of supported tags for the purpose of appending to a condition response message
            /// </summary>
            public void CompileMessageTags()
            {
                string expectationTag = getExpectationTag();

                if (expectationTag != null)
                    Message.Tags.Add(expectationTag);
            }

            /// <summary>
            /// Returns the message tag for this result representing whether the current pass state is expected, or unexpected
            /// when an expectation state is set, null otherwise
            /// </summary>
            private string getExpectationTag()
            {
                if (!HasExpectation())
                    return null;

                return IsUnexpected ? "Unexpected" : "Expected";
            }

            public bool HasExpectation()
            {
                return Expectation != State.None;
            }

            public void Initialize()
            {
                Passed = true;
                Expectation = State.None;
                Message = Message.Empty;
            }

            /// <summary>
            /// Checks that a result is consistent with a set expectation, or if it has passed when none is set
            /// </summary>
            public bool PassedWithExpectations()
            {
                if (!HasExpectation())
                    return Passed;

                return !IsUnexpected;
            }

            public override string ToString()
            {
                string resultMessage = Message.ToString();

                if (resultMessage != null)
                    return resultMessage;
                return string.Empty;
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
