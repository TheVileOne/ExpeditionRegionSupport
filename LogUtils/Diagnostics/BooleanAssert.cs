namespace LogUtils.Diagnostics
{
    public record struct BooleanAssert(IConditionHandler Handler, bool Condition)
    {
        public bool IsTrue()
        {
            bool conditionPassed = Condition == true;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsFalse()
        {
            bool conditionPassed = Condition == false;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }
    }
}
