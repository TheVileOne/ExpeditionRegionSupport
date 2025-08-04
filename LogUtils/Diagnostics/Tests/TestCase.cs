using LogUtils.Helpers.Extensions;
using System;
using System.Collections.Generic;
using MessageFormatter = LogUtils.Diagnostics.AssertHandler.MessageFormatter;

namespace LogUtils.Diagnostics.Tests
{
    public partial class TestCase : IConditionHandler, IDisposable
    {
        /*
         * TestCase and TestCaseGroup function as condition handlers on their own. The purpose is to keep a record of assert results.
         * The results must then be processed and sent to a Logger to view the results. This does not happen automatically.
         * 
         * TestCaseGroup is a composite class that can contain other TestCase objects. If you do not need such functionality, inheriting from
         * TestCase is recommended, it gives you easier access to test results, which would be stored in the child objects in the case of a group.
         * 
         * For best results, compile a report through CreateReport(), report logic has override support.
         * TestCaseGroup is coded to report its own results, and the results of its children. 
         * Reports needs to be handled by a logger to be viewed.
         * 
         * An alternative to using reports is to use an AssertHandler, such as DeferredAssertHandler to collect the assert results. The main
         * advantage of this approach is that it permits results to be processed one at a time allowing the user to curate the result output
         * before it is logged.
         * 
         * Shared state handling: Each test case is responsible for tracking, and handling its own results. An AssertHandler can be provided by overriding
         * TestCase.Handle, but this approach is not supported for test case groups by default. For TestCaseGroup, the user should take advantage of
         * TestCase.GroupState, a special object designed for passing behavior from a group to its children.
         * All children of a TestCaseGroup share a single SharedState passed down from its group instance, while test cases not part of a group have 
         * a separate state instance of their own.
         * ShareState is also how the utility a handler to be passed down from the TestSuite if the user chooses to define a global handler for their
         * test suite. This behavior can be opted out by setting the applicable flag through TestCase.GroupState.
         * In addition to the test suite handler, you can specify a handler specific to a TestCase, or TestCaseGroup through its SharedState instance.
         * In most cases, this is not necessary. This is only if you need the extra functionality, and customization.
         * 
         * Test case results can be handled by multiple handlers, but by default, your test results wont handle themselves. Choose the method of
         * handling results that matches your preferences.
         * 
         * This framework also allows full customization on the result message through the MessageFormatter class. 
         * MessageFormatter is customizable, and can be replaced with a custom one if necessary.
         * The default assertion responses may be found in UtilityConsts, and can be accessed and changed through public fields belonging to a
         * MessageFormatter instance.
         * 
         * Tag System: Tags are strings that can be attached to a result message, which is stored in the Message field of a Condition.Result object.
         * Tags will appear at the end of a result message. Tags allow for easier customization of the result format, while also providing useful
         * information about the result not provided by the message.
         * It is safe to modify the Tags collection, which are stored in Message.Tags.
         */

        public static ILogger TestLogger
        {
            get
            {
                var handler = AssertHandler.GetCompatibleTemplate(TestSuite.ActiveSuite?.Handler);
                return handler.Logger;
            }
        }

        public MessageFormatter Formatter;

        /// <summary>
        /// The group that this test case belongs to
        /// </summary>
        public readonly TestCaseGroup Group;

        /// <summary>
        /// A shared state that applies to the top level test case and all of its children
        /// </summary>
        public readonly SharedState GroupState;

        public IReadOnlyList<IConditionHandler> ApplicableHandlers => GroupState.GetApplicableHandlers(this);

        public virtual bool IsEnabled => Debug.AssertsEnabled;

        public string Name { get; }

        public List<Condition.Result> Results;

        public TestCase(string name) : this(name, new SharedState())
        {
        }

        public TestCase(TestCaseGroup group, string name) : this(name, group.GroupState)
        {
            Group = group;
            Group.Add(this);
        }

        protected TestCase(string name, SharedState state)
        {
            Name = name;
            GroupState = state;
            Formatter = new MessageFormatter();
            Results = new List<Condition.Result>();
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T> AssertThat<T>(T value)
        {
            var condition = Assert.That(value, this);

            condition.AddHandlers(ApplicableHandlers);
            return condition;
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T?> AssertThat<T>(T? value) where T : struct
        {
            var condition = Assert.That(value, this);

            condition.AddHandlers(ApplicableHandlers);
            return condition;
        }

        /// <summary>
        /// Selects a new test case from the parent group
        /// </summary>
        public void Dispose()
        {
            //Alert the case group that this case is finished handling cases, and the next test can take over
            if (Group != null)
                Group.NextCase();
        }

        public virtual void Handle(Condition.Result result)
        {
            Results.Add(result);
        }

        /// <summary>
        /// Checks that the test case has a failed outcome
        /// </summary>
        public virtual bool HasFailed()
        {
            var analyzer = Results.GetAnalyzer();
            return analyzer.HasFailedResults();
        }

        /// <summary>
        /// State that is capable of being shared between two or more test case instances
        /// </summary>
        public class SharedState
        {
            /// <summary>
            /// Should the state of the TestSuit affect the state of this instance
            /// </summary>
            public bool InheritFromTestSuite = true;

            /// <summary>
            /// Should children be exposed to this state, or only the most top level instance
            /// </summary>
            public bool PropagateToChildren = true;

            public IConditionHandler SharedHandler;

            public List<IConditionHandler> GetApplicableHandlers(TestCase instance)
            {
                var handlers = new List<IConditionHandler>();

                bool isChildInstance = instance.Group != null;

                if (!isChildInstance || PropagateToChildren)
                {
                    if (SharedHandler != null)
                        handlers.Add(SharedHandler);

                    if (InheritFromTestSuite)
                    {
                        var suiteHandler = TestSuite.ActiveSuite?.Handler;

                        if (suiteHandler != null)
                            handlers.Add(suiteHandler);
                    }
                }
                return handlers;
            }
        }
    }
}
