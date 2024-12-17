using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public record struct NumericAssert(List<IConditionHandler> Handlers, double Value)
    {
        public bool IsEqualTo(double checkValue)
        {
            var result = Assert.IsEqual(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool DoesNotEqual(double checkValue)
        {
            var result = Assert.DoesNotEqual(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsGreaterThan(double checkValue)
        {
            var result = Assert.IsGreaterThan(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsGreaterThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsGreaterThanOrEqualTo(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsLessThan(double checkValue)
        {
            var result = Assert.IsLessThan(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsLessThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsLessThanOrEqualTo(Value, checkValue);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsBetween(double checkValue, double checkValue2)
        {
            var result = Assert.IsBetween(Value, checkValue, checkValue2);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsZero()
        {
            var result = Assert.IsZero(Value);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(Func<double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(Value, condition, criteria);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="checkValue">A value to be used for the evaluation process</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(double checkValue, Func<double, double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(Value, checkValue, condition, criteria);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }
    }

    public enum EvaluationCriteria
    {
        MustBeTrue,
        MustBeFalse
    }
}
