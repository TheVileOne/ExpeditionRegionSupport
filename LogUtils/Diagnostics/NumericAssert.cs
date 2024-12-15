using System;

namespace LogUtils.Diagnostics
{
    public record struct NumericAssert(IConditionHandler Handler, double Value)
    {
        public bool IsEqualTo(double checkValue)
        {
            bool conditionPassed = Value == checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNotEqualTo(double checkValue)
        {
            bool conditionPassed = Value != checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsGreaterThan(double checkValue)
        {
            bool conditionPassed = Value > checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsGreaterThanOrEqualTo(double checkValue)
        {
            bool conditionPassed = Value >= checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsLessThan(double checkValue)
        {
            bool conditionPassed = Value < checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsLessThanOrEqualTo(double checkValue)
        {
            bool conditionPassed = Value <= checkValue;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsBetween(double checkValue, double checkValue2)
        {
            bool conditionPassed = false;
            if (checkValue != checkValue2)
            {
                double lowBound, highBound;

                if (checkValue < checkValue2)
                {
                    lowBound = checkValue;
                    highBound = checkValue2;
                }
                else
                {
                    lowBound = checkValue2;
                    highBound = checkValue;
                }
                conditionPassed = Value > lowBound && Value < highBound;
            }

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsZero()
        {
            bool conditionPassed = Value == 0;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNaN()
        {
            bool conditionPassed = double.IsNaN(Value);

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(Func<double, bool> condition, EvaluationCriteria criteria)
        {
            bool conditionIsTrue = condition(Value);

            conditionIsTrue =
                   (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            Assert.OnResult(Handler, new ConditionResults(null, conditionIsTrue));
            return conditionIsTrue;
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
            bool conditionIsTrue = condition(Value, checkValue);

            conditionIsTrue =
                   (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            Assert.OnResult(Handler, new ConditionResults(null, conditionIsTrue));
            return conditionIsTrue;
        }
    }

    public enum EvaluationCriteria
    {
        MustBeTrue,
        MustBeFalse
    }
}
