using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Diagnostics
{
    public struct Condition<T>
    {
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
            //TODO: Pass condition to handler
        }
    }
}
