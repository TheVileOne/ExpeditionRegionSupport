using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public record struct ObjectAssert(List<IConditionHandler> Handlers, object Data)
    {
        public bool IsEqualTo(object checkData)
        {
            var result = Assert.IsEqual(Data, checkData);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool DoesNotEqual(object checkData)
        {
            var result = Assert.DoesNotEqual(Data, checkData);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(Data);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(Data);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }
    }
}
