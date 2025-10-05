using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public class DeferredAssertHandler : AssertHandler
    {
        protected Queue<Condition.Result> Results = new Queue<Condition.Result>();

        /// <summary>
        /// Peeks at the current result in the result queue
        /// </summary>
        /// <exception cref="InvalidOperationException">The result queue is empty</exception>
        public Condition.Result Current => Results.Peek();

        public bool HasResults => Results.Any();

        public DeferredAssertHandler(ILogger logger) : base(logger)
        {
        }

        /// <summary>
        /// Enqueues the result in a result queue for future handling
        /// </summary>
        public override void Handle(Condition.Result result)
        {
            Results.Enqueue(result);
        }

        /// <summary>
        /// Handles all results
        /// </summary>
        public DeferredAssertHandler HandleAll()
        {
            while (HasResults)
                InternalHandle();
            return this;
        }

        /// <summary>
        /// Handles all results
        /// </summary>
        /// <remarks>When delegate returns true, result is handled normally, when false result is discarded</remarks>
        public DeferredAssertHandler HandleAll(HandleCondition handleWhenTrue)
        {
            while (HasResults)
                InternalHandle(handleWhenTrue);
            return this;
        }

        /// <summary>
        /// Handles the currently enumerated result
        /// </summary>
        public DeferredAssertHandler HandleCurrent()
        {
            if (HasResults)
                InternalHandle();
            return this;
        }

        public DeferredAssertHandler HandleCurrent(Condition.State expectation)
        {
            if (HasResults)
            {
                var result = Current;

                result.Expectation = expectation;
                InternalHandle();
            }
            return this;
        }

        /// <summary>
        /// Handles the currently enumerated result
        /// </summary>
        /// <remarks>When delegate returns true, result is handled normally, when false result is discarded</remarks>
        public DeferredAssertHandler HandleCurrent(HandleCondition handleWhenTrue)
        {
            if (HasResults)
                InternalHandle(handleWhenTrue);
            return this;
        }

        protected void InternalHandle()
        {
            base.Handle(Results.Dequeue());
        }

        protected void InternalHandle(HandleCondition handleWhenTrue)
        {
            var shouldHandle = handleWhenTrue;

            if (shouldHandle(Current))
            {
                InternalHandle();
                return;
            }
            Results.Dequeue();
        }

        public delegate bool HandleCondition(Condition.Result result);
    }
}
