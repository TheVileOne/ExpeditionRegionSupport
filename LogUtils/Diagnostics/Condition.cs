using System.Collections.Generic;
using static LogUtils.Diagnostics.Debug;

namespace LogUtils.Diagnostics
{
    public static class Condition
    {
        public static BooleanAssert Check(bool condition) => new BooleanAssert(condition);
        public static NumericAssert Check(double value) => new NumericAssert(value);
        public static CollectionAssert<T> Check<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(enumerable);
        public static ObjectAssert Check(object data) => new ObjectAssert(data);

        public static BooleanAssert Assert(bool condition) => new BooleanAssert(condition);
        public static NumericAssert Assert(double value) => new NumericAssert(value);
        public static CollectionAssert<T> Assert<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(enumerable);
        public static ObjectAssert Assert(object data) => new ObjectAssert(data);
    }
}
