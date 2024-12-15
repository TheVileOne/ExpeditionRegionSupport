namespace LogUtils.Diagnostics
{
    public record struct ObjectAssert(IConditionHandler Handler, object Data)
    {
        public bool IsEqualTo(object checkData)
        {
            bool conditionPassed = Equals(Data, checkData);
            
            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNotEqualTo(double checkValue)
        {
            bool conditionPassed = !Equals(Data, checkValue);

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNull()
        {
            bool conditionPassed = Data == null;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNotNull()
        {
            bool conditionPassed = Data != null;

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }
    }
}
