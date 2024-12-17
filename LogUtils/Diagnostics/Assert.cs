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
                PassMessage = UtilityConsts.AssertMessages.CONDITION_TRUE,
                FailMessage = UtilityConsts.AssertMessages.CONDITION_FALSE
            };

            result.Descriptors.Add("Condition");
            return result;
        }

        public static ConditionResults IsFalse(bool condition)
        {
            bool conditionPassed = condition == false;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.CONDITION_FALSE,
                FailMessage = UtilityConsts.AssertMessages.CONDITION_TRUE
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
                PassMessage = UtilityConsts.AssertMessages.VALUES_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUES_NOT_EQUAL
            };

            result.Descriptors.Add("Objects");
            return result;
        }

        public static ConditionResults DoesNotEqual(object obj, object obj2)
        {
            bool conditionPassed = !object.Equals(obj, obj2);
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUES_NOT_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUES_EQUAL
            };

            result.Descriptors.Add("Objects");
            return result;
        }

        public static ConditionResults IsEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = value.Equals(value2);
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUES_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUES_NOT_EQUAL
            };

            result.Descriptors.Add("Value types");
            return result;
        }

        public static ConditionResults DoesNotEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = !value.Equals(value2);
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUES_NOT_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUES_EQUAL
            };

            result.Descriptors.Add("Value types");
            return result;
        }

        public static ConditionResults IsNull(object obj)
        {
            bool conditionPassed = obj == null;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_NULL,
                FailMessage = UtilityConsts.AssertMessages.VALUE_NOT_NULL
            };

            result.Descriptors.Add("Object");
            return result;
        }

        public static ConditionResults IsNotNull(object obj)
        {
            bool conditionPassed = obj != null;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_NOT_NULL,
                FailMessage = UtilityConsts.AssertMessages.VALUE_NULL
            };

            result.Descriptors.Add("Object");
            return result;
        }
        #endregion
        #region Collections
        public static ConditionResults IsEmpty<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = !collection.Any();
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.COLLECTION_EMPTY,
                FailMessage = UtilityConsts.AssertMessages.COLLECTION_HAS_ITEMS
            };

            result.Descriptors.Add("Collection");
            return result;
        }

        public static ConditionResults HasItems<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = collection.Any();
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.COLLECTION_HAS_ITEMS,
                FailMessage = UtilityConsts.AssertMessages.COLLECTION_EMPTY
            };

            result.Descriptors.Add("Collection");
            return result;
        }
        #endregion
        #region Numerics
        public static ConditionResults IsGreaterThan(double value, double value2)
        {
            bool conditionPassed = value > value2;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_ABOVE,
                FailMessage = UtilityConsts.AssertMessages.VALUE_NOT_ABOVE
            };

            result.SetDescriptors("Value", value2.ToString());
            return result;
        }

        public static ConditionResults IsGreaterThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_ABOVE_OR_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUE_BELOW
            };

            result.SetDescriptors("Value", value2.ToString());
            return result;
        }

        public static ConditionResults IsLessThan(double value, double value2)
        {
            bool conditionPassed = value < value2;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_BELOW,
                FailMessage = UtilityConsts.AssertMessages.VALUE_NOT_BELOW
            };

            result.SetDescriptors("Value", value2.ToString());
            return result;
        }

        public static ConditionResults IsLessThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_BELOW_OR_EQUAL,
                FailMessage = UtilityConsts.AssertMessages.VALUE_ABOVE
            };

            result.SetDescriptors("Value", value2.ToString());
            return result;
        }

        public static ConditionResults IsBetween(double value, double bound, double bound2)
        {
            bool conditionPassed = false;
            if (bound != bound2)
            {
                double lowBound, highBound;

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

            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_IN_RANGE,
                FailMessage = UtilityConsts.AssertMessages.VALUE_OUT_OF_RANGE
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsZero(double value)
        {
            bool conditionPassed = value == 0;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_ZERO,
                FailMessage = UtilityConsts.AssertMessages.VALUE_NOT_ZERO
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsNotZero(double value)
        {
            bool conditionPassed = value != 0;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_NOT_ZERO,
                FailMessage = UtilityConsts.AssertMessages.VALUE_ZERO
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsNegative(double value)
        {
            bool conditionPassed = value < 0;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_NEGATIVE,
                FailMessage = UtilityConsts.AssertMessages.UNEXPECTED_VALUE
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsPositive(double value)
        {
            bool conditionPassed = value > 0;
            var result = new ConditionResults(conditionPassed)
            {
                FailMessage = UtilityConsts.AssertMessages.VALUE_ZERO
            };

            result.Descriptors.Add("Value");
            return result;
        }

        public static ConditionResults IsPositiveOrZero(double value)
        {
            bool conditionPassed = value >= 0;
            var result = new ConditionResults(conditionPassed)
            {
                PassMessage = UtilityConsts.AssertMessages.VALUE_NOT_ZERO,
                FailMessage = UtilityConsts.AssertMessages.VALUE_ZERO
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

            return new ConditionResults(conditionIsTrue);
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

            return new ConditionResults(conditionIsTrue);
        }
        #endregion
    }
}
