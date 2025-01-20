using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCase : IConditionHandler, IDisposable
    {
        private TestCaseGroup _group;

        /// <summary>
        /// The group that this test case belongs to
        /// </summary>
        public TestCaseGroup Group => _group;

        /// <summary>
        /// The result processor specific to this test case or its children. Null by default
        /// </summary>
        public IConditionHandler Handler;

        public virtual bool HasFailed => Results.Exists(r => !r.PassedWithExpectations());

        public virtual bool IsEnabled => Debug.AssertsEnabled;

        public string Name { get; }

        public List<Condition.Result> Results;

        public TestCase(string name) : this(null, name, null)
        {
        }

        public TestCase(string name, IConditionHandler handler) : this(null, name, handler)
        {
        }

        public TestCase(TestCaseGroup group, string name) : this(group, name, null)
        {
        }

        public TestCase(TestCaseGroup group, string name, IConditionHandler handler)
        {
            if (group != null)
                group.Add(this);

            Handler = handler ?? this;
            Name = name;
            Results = new List<Condition.Result>();
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T> AssertThat<T>(T value)
        {
            return Assert.That(value, Handler);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T?> AssertThat<T>(T? value) where T : struct
        {
            return Assert.That(value, Handler);
        }

        public void Dispose()
        {
            //Alert the case group that this case is finished handling cases, and the next test can take over
            if (Group != null)
                Group.NextCase();
        }

        protected virtual List<IConditionHandler> GetInheritedHandlers()
        {
            List<IConditionHandler> handlers = new List<IConditionHandler>();
            if (Group != null)
            {
                if (Group.Handler != null)
                    handlers.Add(Group.Handler);
                handlers.AddRange(Group.GetInheritedHandlers());
            }
            return handlers;
        }

        public virtual void Handle(in Condition.Result result)
        {
            Results.Add(result);
        }

        internal void SetGroupFromParent(TestCaseGroup group)
        {
            _group = group;
        }
    }
}
