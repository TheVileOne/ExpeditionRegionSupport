using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public static class Condition
    {
        public static IConditionHandler CheckHandler = null;
        public static IConditionHandler AssertHandler = new AssertHandler();

        public static BooleanAssert Check(bool condition) => new BooleanAssert(CheckHandler, condition);
        public static NumericAssert Check(double value) => new NumericAssert(CheckHandler, value);
        public static CollectionAssert<T> Check<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(CheckHandler, enumerable);
        public static ObjectAssert Check(object data) => new ObjectAssert(CheckHandler, data);

        public static BooleanAssert Assert(bool condition) => new BooleanAssert(AssertHandler, condition);
        public static NumericAssert Assert(double value) => new NumericAssert(AssertHandler, value);
        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(AssertHandler, enumerable);
        public static ObjectAssert Assert(object data) => new ObjectAssert(AssertHandler, data);
    }
}
