using AssertDocs = LogUtils.Documentation.AssertDocumentation;
using AssertResponse = LogUtils.UtilityConsts.AssertResponse;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.ObjectAssert.DoesNotEqual{T}(Condition{T}, T)"/>
        public static Condition<T> DoesNotEqual<T>(this Condition<T> condition, T compareObject)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;

            bool hasValue = condition.Value != null;
            bool hasValueOther = compareObject != null;

            if (!hasValue || !hasValueOther) //One or both of these values are null
            {
                conditionPassed = hasValue != hasValueOther;
            }
            else //Avoid boxing, by handling potential value types here
            {
                conditionPassed = !condition.Value.Equals(compareObject);
            }

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptor = "Objects";

                if (typeof(T).IsValueType)
                    reportDescriptor = "Values";

                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_EQUAL, reportDescriptor));
            }
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.DoesNotEqual{T}(Condition{T}, T)"/>
        public static Condition<T> DoesNotEqual<T>(this Condition<T> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = compareObject == null || !condition.Value.Equals(compareObject.Value);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_EQUAL, "Values"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.DoesNotEqual{T}(Condition{T}, T)"/>
        public static Condition<T?> DoesNotEqual<T>(this Condition<T?> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;

            bool hasValue = condition.Value.HasValue;
            bool hasValueOther = compareObject.HasValue;

            if (hasValue != hasValueOther) //One of these values are null, but not both
            {
                conditionPassed = true;
            }
            else if (hasValue) //Both values must not be null
            {
                T value = condition.Value.Value;
                conditionPassed = !value.Equals(compareObject.Value);
            }
            else //Both values must be null
            {
                conditionPassed = false;
            }

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_EQUAL, "Values"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsEqualTo{T}(Condition{T}, T)"/>
        public static Condition<T> IsEqualTo<T>(this Condition<T> condition, T compareObject)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;

            bool hasValue = condition.Value != null;
            bool hasValueOther = compareObject != null;

            if (!hasValue || !hasValueOther) //One or both of these values are null
            {
                conditionPassed = hasValue == hasValueOther;
            }
            else //Avoid boxing, by handling potential value types here
            {
                conditionPassed = condition.Value.Equals(compareObject);
            }

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptor = "Objects";

                if (typeof(T).IsValueType)
                    reportDescriptor = "Values";

                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EQUAL, reportDescriptor));
            }
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsEqualTo{T}(Condition{T}, T)"/>
        public static Condition<T> IsEqualTo<T>(this Condition<T> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = compareObject != null && condition.Value.Equals(compareObject.Value);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EQUAL, "Values"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsEqualTo{T}(Condition{T}, T)"/>
        public static Condition<T?> IsEqualTo<T>(this Condition<T?> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;

            bool hasValue = condition.Value.HasValue;
            bool hasValueOther = compareObject.HasValue;

            if (hasValue != hasValueOther) //One of these values are null, but not both
            {
                conditionPassed = false;
            }
            else if (hasValue) //Both values must not be null
            {
                T value = condition.Value.Value;
                conditionPassed = value.Equals(compareObject.Value);
            }
            else //Both values must be null
            {
                conditionPassed = true;
            }

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EQUAL, "Values"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsNotNull{T}(Condition{T})"/>
        public static Condition<T> IsNotNull<T>(this Condition<T> condition) where T : class
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value != null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Object"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsNotNull{T}(Condition{T})"/>
        public static Condition<T?> IsNotNull<T>(this Condition<T?> condition) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value != null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Nullable value"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsNotThisInstance{T}(Condition{T}, T)"/>
        public static Condition<T> IsNotThisInstance<T>(this Condition<T> condition, T compareObject) where T : class
        {
            bool conditionPassed = !ReferenceEquals(condition.Value, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_SAME_INSTANCE, "Object"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsNull{T}(Condition{T})"/>
        public static Condition<T> IsNull<T>(this Condition<T> condition) where T : class
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_NULL, "Object"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsNull{T}(Condition{T})"/>
        public static Condition<T?> IsNull<T>(this Condition<T?> condition) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_NULL, "Nullable value"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.ObjectAssert.IsSameInstance{T}(Condition{T}, T)"/>
        public static Condition<T> IsSameInstance<T>(this Condition<T> condition, T compareObject) where T : class
        {
            bool conditionPassed = ReferenceEquals(condition.Value, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_SAME_INSTANCE, "Object"));
            return condition;
        }
    }
}