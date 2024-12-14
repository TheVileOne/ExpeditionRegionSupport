using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public static class Debug
    {
        public record struct CollectionAssert<T>(IEnumerable<T> enumerable)
        {
            public bool IsEmpty()
            {
                return !enumerable.Any();
            }

            public bool HasItems()
            {
                return enumerable.Any();
            }

            public bool IsNull()
            {
                return new ObjectAssert(enumerable).IsNull();
            }

            public bool IsNotNull()
            {
                return new ObjectAssert(enumerable).IsNotNull();
            }
        }

        public record struct ObjectAssert(object data)
        {
            public bool IsEqualTo(object checkData)
            {
                return Equals(data, checkData);
            }

            public bool IsNotEqualTo(double checkValue)
            {
                return !Equals(data, checkValue);
            }

            public bool IsNull()
            {
                return data == null;
            }

            public bool IsNotNull()
            {
                return data != null;
            }
        }

        public record struct BooleanAssert(bool condition)
        {
            public bool IsTrue()
            {
                return condition == true;
            }

            public bool IsFalse()
            {
                return condition == false;
            }
        }

        public record struct NumericAssert(double value)
        {
            public bool IsEqualTo(double checkValue)
            {
                return value == checkValue;
            }

            public bool IsNotEqualTo(double checkValue)
            {
                return value != checkValue;
            }

            public bool IsGreaterThan(double checkValue)
            {
                return value > checkValue;
            }

            public bool IsGreaterThanOrEqualTo(double checkValue)
            {
                return value >= checkValue;
            }

            public bool IsLessThan(double checkValue)
            {
                return value < checkValue;
            }

            public bool IsLessThanOrEqualTo(double checkValue)
            {
                return value <= checkValue;
            }

            public bool IsBetween(double checkValue, double checkValue2)
            {
                if (checkValue == checkValue2) return false;

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
                return value > lowBound && value < highBound;
            }

            public bool IsZero()
            {
                return value == 0;
            }

            public bool IsNaN()
            {
                return double.IsNaN(value);
            }

            /// <summary>
            /// Uses the provided check condition delegate to assert a condition
            /// </summary>
            /// <param name="condition">A delegate that evaluates the assigned value</param>
            /// <param name="criteria">The expected state of the condition</param>
            /// <returns>true, if the condition state matches expectations, otherwise false</returns>
            public bool EvaluateCondition(Func<double, bool> condition, EvaluationCriteria criteria)
            {
                bool conditionIsTrue = condition(value);

                return (criteria == EvaluationCriteria.MustBeTrue  && conditionIsTrue)
                    || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);
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
                bool conditionIsTrue = condition(value, checkValue);

                return (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                    || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);
            }
        }

        public enum EvaluationCriteria
        {
            MustBeTrue,
            MustBeFalse
        }
    }
}
