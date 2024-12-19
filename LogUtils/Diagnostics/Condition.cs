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

        public static BooleanAssert Assert(bool condition)                            => new BooleanAssert(condition, assertArgs: default);
        public static BooleanAssert Assert(bool condition, AssertArgs assertArgs)     => new BooleanAssert(condition, assertArgs);
        public static BooleanAssert Assert(bool condition, IConditionHandler handler) => new BooleanAssert(condition, new AssertArgs(handler));

        public static NumericAssert Assert(double value)                              => new NumericAssert(value, assertArgs: default);
        public static NumericAssert Assert(double value, AssertArgs assertArgs)       => new NumericAssert(value, assertArgs);
        public static NumericAssert Assert(double value, IConditionHandler handler)   => new NumericAssert(value, new AssertArgs(handler));

        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable)                            => new CollectionAssert<T>(enumerable, assertArgs: default);
        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable, AssertArgs assertArgs)     => new CollectionAssert<T>(enumerable, assertArgs);
        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable, IConditionHandler handler) => new CollectionAssert<T>(enumerable, new AssertArgs(handler));

        public static ObjectAssert Assert(object data)                            => new ObjectAssert(data, assertArgs: default);
        public static ObjectAssert Assert(object data, AssertArgs assertArgs)     => new ObjectAssert(data, assertArgs);
        public static ObjectAssert Assert(object data, IConditionHandler handler) => new ObjectAssert(data, new AssertArgs(handler));
    }
}
