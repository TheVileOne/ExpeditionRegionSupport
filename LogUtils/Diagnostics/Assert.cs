using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public static class Assert
    {
        internal static void OnResult(List<IConditionHandler> handlers, ConditionResults result)
        {
            foreach (var handler in handlers)
                handler.Handle(result);
        }

        #region Boolean
        public static ConditionResults IsTrue(bool condition)
        {
            bool conditionPassed = condition == true;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE
            };

            result.Descriptors.Add("Condition");
            return result;
        }

        public static ConditionResults IsFalse(bool condition)
        {
            bool conditionPassed = condition == false;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE
            };

            result.Descriptors.Add("Condition");
            return result;
        }
        #endregion
        #region Objects and Structs
        public static ConditionResults IsEqual(object obj, object obj2)
        {
            bool conditionPassed = object.Equals(obj, obj2);
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_EQUAL
            };

            result.Descriptors.Add("Objects");
            return result;
        }

        public static ConditionResults DoesNotEqual(object obj, object obj2)
        {
            bool conditionPassed = !object.Equals(obj, obj2);
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL
            };

            result.Descriptors.Add("Objects");
            return result;
        }

        public static ConditionResults IsEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = value.Equals(value2);
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_EQUAL
            };

            result.Descriptors.Add("Value types");
            return result;
        }

        public static ConditionResults DoesNotEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = !value.Equals(value2);
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL
            };

            result.Descriptors.Add("Value types");
            return result;
        }

        public static ConditionResults IsNull(object obj)
        {
            bool conditionPassed = obj == null;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_NULL
            };

            result.Descriptors.Add("Object");
            return result;
        }

        public static ConditionResults IsNotNull(object obj)
        {
            bool conditionPassed = obj != null;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_NOT_BE_NULL
            };

            result.Descriptors.Add("Object");
            return result;
        }
        #endregion
        #region Collections
        public static ConditionResults IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = collection == null || !collection.Any();
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_EMPTY
            };

            result.Descriptors.Add("Collection");
            return result;
        }

        public static ConditionResults HasItems<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = collection != null && collection.Any();
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_HAVE_ITEMS
            };

            result.Descriptors.Add("Collection");
            return result;
        }
        #endregion
        #region Numerics
        public static ConditionResults IsEqual(double value, double value2)
        {
            int valueDiff = value.CompareTo(value2);

            bool conditionPassed = valueDiff == 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsGreaterThan(double value, double value2)
        {
            bool conditionPassed = value > value2;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.TOO_LOW
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsGreaterThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.TOO_LOW
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsLessThan(double value, double value2)
        {
            bool conditionPassed = value < value2;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.TOO_HIGH
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsLessThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.TOO_HIGH
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsBetween(double value, double bound, double bound2)
        {
            bool conditionPassed = false;
            double lowBound, highBound;
            if (bound != bound2)
            {
                if (bound < bound2)
                {
                    lowBound = bound;
                    highBound = bound2;
                }
                else
                {
                    lowBound = bound2;
                    highBound = bound;
                }
                conditionPassed = value > lowBound && value < highBound;
            }
            else
                lowBound = highBound = bound;

            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_IN_RANGE
            };

            result.SetDescriptors("Value", lowBound.ToString(), highBound.ToString());
            return result;
        }

        public static ConditionResults IsZero(double value)
        {
            int valueDiff = value.CompareTo(0);

            bool conditionPassed = valueDiff == 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsNotZero(double value)
        {
            int valueDiff = value.CompareTo(0);

            bool conditionPassed = valueDiff != 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_NOT_BE_ZERO
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsNegative(double value)
        {
            bool conditionPassed = value < 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_NEGATIVE
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsPositive(double value)
        {
            bool conditionPassed = value > 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_BE_POSITIVE
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsPositiveOrZero(double value)
        {
            bool conditionPassed = value >= 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertResponse.MUST_NOT_BE_NEGATIVE
            };

            result.Descriptors.Add("Value");
            return result;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static ConditionResults EvaluateCondition(double value, Func<double, bool> condition, EvaluationCriteria criteria)
        {
            bool conditionIsTrue = condition(value);

            conditionIsTrue =
                   (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            string failMessage = null;
            if (criteria == EvaluationCriteria.MustBeTrue)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE;
            else if (criteria == EvaluationCriteria.MustBeFalse)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE;

            var result = new ConditionResults(conditionIsTrue)
            {
                FailMessage = failMessage
            };

            if (failMessage != null)
                result.Descriptors.Add("Condition");
            return result;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="value2">A value to be used for the evaluation process</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static ConditionResults EvaluateCondition(double value, double value2, Func<double, double, bool> condition, EvaluationCriteria criteria)
        {
            bool conditionIsTrue = condition(value, value2);

            conditionIsTrue =
                   (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            string failMessage = null;
            if (criteria == EvaluationCriteria.MustBeTrue)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE;
            else if (criteria == EvaluationCriteria.MustBeFalse)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE;

            var result = new ConditionResults(conditionIsTrue)
            {
                FailMessage = failMessage
            };

            if (failMessage != null)
                result.Descriptors.Add("Condition");
            return result;
        }
        #endregion
    }
}
