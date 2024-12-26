using LogUtils.Helpers;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public struct AssertArgs
    {
        /// <summary>
        /// What to do on a failed assertion
        /// </summary>
        public AssertBehavior Behavior;

        /// <summary>
        /// Set if you want only this handler to handle an assert
        /// </summary>
        public IConditionHandler ExclusiveHandler;

        public AssertArgs()
        {
        }

        public AssertArgs(IConditionHandler handler) : this(handler, AssertBehavior.Log)
        {
        }

        public AssertArgs(AssertBehavior behavior) : this(null, behavior)
        {
        }

        public AssertArgs(IConditionHandler handler, AssertBehavior behavior)
        {
            ExclusiveHandler = handler;
            Behavior = behavior;
        }
    }
}
