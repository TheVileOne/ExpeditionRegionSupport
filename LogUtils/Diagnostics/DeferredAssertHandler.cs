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

        /// <summary>
        /// An optional field for analyzing expected outcomes in result data
        /// </summary>
        private Condition.State expectedResult = Condition.State.None;

        public DeferredAssertHandler(Logger logger) : base(logger)
        {
        }

        /// <summary>
        /// Enqueues the result in a result queue for future handling
        /// </summary>
        public override void Handle(in Condition.Result result)
        {
            Results.Enqueue(result);
        }

        /// <summary>
        /// Handles all results
        /// </summary>
        public void HandleAll()
        {
            while (HasResults)
                base.Handle(Results.Dequeue());
        }

        /// <summary>
        /// Handles all results
        /// <br></br>
        /// <br>When delegate returns true, result is handled normally, when false result is discarded</br>
        /// </summary>
        public void HandleAll(HandleCondition handleWhenTrue)
        {
            var shouldHandle = handleWhenTrue;

            while (HasResults)
            {
                Condition.Result result = Results.Dequeue();

                if (shouldHandle(result))
                    base.Handle(result);
            }
        }

        /// <summary>
        /// Handles the currently enumerated result
        /// </summary>
        public void HandleCurrent()
        {
            if (HasResults)
                base.Handle(Results.Dequeue());
        }

        public void HandleCurrent(Condition.State expectation)
        {
            if (HasResults)
            {
                expectedResult = expectation;

                //Peeks at the current result instead of dequeues, because we need to reference the result in another method before we can remove it
                base.Handle(Current);

                Results.Dequeue();
                expectedResult = Condition.State.None;
            }
        }

        /// <summary>
        /// Handles the currently enumerated result
        /// <br></br>
        /// <br>When delegate returns true, result is handled normally, when false result is discarded</br>
        /// </summary>
        public void HandleCurrent(HandleCondition handleWhenTrue)
        {
            var shouldHandle = handleWhenTrue;

            if (HasResults)
            {
                Condition.Result result = Results.Dequeue();

                if (shouldHandle(result))
                    base.Handle(result);
            }
        }

        protected override void PostProcessResponseString(ref string response)
        {
            base.PostProcessResponseString(ref response);

            switch (expectedResult)
            {
                case Condition.State.None:
                    break;
                case Condition.State.Pass:
                    response += Current.Passed ? " (Expected)" : " (Unexpected)";
                    break;
                case Condition.State.Fail:
                    response += !Current.Passed ? " (Expected)" : " (Unexpected)";
                    break;
            }
        }

        public delegate bool HandleCondition(Condition.Result result);
    }
}
