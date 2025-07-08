using LogUtils.Enums;
using static LogUtils.UtilityConsts;
using AssertDocs = LogUtils.Documentation.AssertDocumentation;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.OtherAssert.IsEmpty(Condition{CompositeLogCategory})"/>
        public static Condition<CompositeLogCategory> IsEmpty(this Condition<CompositeLogCategory> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value.IsEmpty;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EMPTY, "Composite ExtEnum"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.OtherAssert.Contains(Condition{CompositeLogCategory}, LogCategory)"/>
        public static Condition<CompositeLogCategory> Contains(this Condition<CompositeLogCategory> condition, LogCategory flag)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value.Contains(flag);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_CONTAIN, "Composite ExtEnum", flag.ToString()));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.OtherAssert.ContainsOnly(Condition{CompositeLogCategory}, LogCategory)"/>
        public static Condition<CompositeLogCategory> ContainsOnly(this Condition<CompositeLogCategory> condition, LogCategory flag)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value.FlagCount == 1 && condition.Value.Contains(flag);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_ONLY_CONTAIN, "Composite ExtEnum", $"the value {flag}"));
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.OtherAssert.DoesNotContain(Condition{CompositeLogCategory}, LogCategory)"/>
        public static Condition<CompositeLogCategory> DoesNotContain(this Condition<CompositeLogCategory> condition, LogCategory flag)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = !condition.Value.Contains(flag);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_CONTAIN, "Composite ExtEnum", flag.ToString()));
            return condition;
        }
    }
}
