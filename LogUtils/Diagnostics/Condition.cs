using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public static class Condition
    {
        public static readonly List<IConditionHandler> CheckHandlers = new List<IConditionHandler>();
        public static readonly List<IConditionHandler> AssertHandlers = new List<IConditionHandler>();

        public static BooleanAssert Check(bool condition) => new BooleanAssert(CheckHandlers, condition);
        public static NumericAssert Check(double value) => new NumericAssert(CheckHandlers, value);
        public static CollectionAssert<T> Check<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(CheckHandlers, enumerable);
        public static ObjectAssert Check(object data) => new ObjectAssert(CheckHandlers, data);

        public static BooleanAssert Assert(bool condition) => new BooleanAssert(AssertHandlers, condition);
        public static NumericAssert Assert(double value) => new NumericAssert(AssertHandlers, value);
        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(AssertHandlers, enumerable);
        public static ObjectAssert Assert(object data) => new ObjectAssert(AssertHandlers, data);
    }
}
