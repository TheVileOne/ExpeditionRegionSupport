using AssertDocs = LogUtils.Documentation.AssertDocumentation;
using AssertResponse = LogUtils.UtilityConsts.AssertResponse;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.BooleanAssert.IsFalse(Condition{bool})"/>
        public static Condition<bool> IsFalse(this Condition<bool> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == false;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_FALSE, "Condition"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.BooleanAssert.IsFalse(Condition{bool})"/>
        public static Condition<bool?> IsFalse(this Condition<bool?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            //Null is considered different than false
            bool conditionPassed = condition.Value.HasValue && condition.Value == false;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_FALSE, "Condition"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.BooleanAssert.IsTrue(Condition{bool})"/>
        public static Condition<bool> IsTrue(this Condition<bool> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == true;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_TRUE, "Condition"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.BooleanAssert.IsTrue(Condition{bool})"/>
        public static Condition<bool?> IsTrue(this Condition<bool?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value.HasValue && condition.Value == true;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_TRUE, "Condition"));
            return condition;
        }
    }
}