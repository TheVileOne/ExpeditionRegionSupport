using LogUtils.Helpers;
using System;
using System.Collections.Generic;
using MessageFormatter = LogUtils.Diagnostics.AssertHandler.MessageFormatter;

namespace LogUtils.Diagnostics.Tests
{
    public partial class TestCase : IConditionHandler, IDisposable
    {
        public MessageFormatter Formatter;

        /// <summary>
        /// The group that this test case belongs to
        /// </summary>
        public readonly TestCaseGroup Group;

        /// <summary>
        /// A shared state that applies to the top level test case and all of its children
        /// </summary>
        public readonly SharedState GroupState;

        /// <summary>
        /// The result processor specific to this test case or its children. Null by default
        /// </summary>
        public IConditionHandler Handler;

        public IReadOnlyList<IConditionHandler> ApplicableHandlers
        {
            get
            {
                var handlers = GroupState.GetApplicableHandlers(this);

                if (Handler != null)
                    handlers.Add(Handler);
                return handlers;
            }
        }

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
