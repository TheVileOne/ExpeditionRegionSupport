using System;

namespace ExpeditionRegionSupport.ExceptionHandling
{
    public class ExceptionHandler
    {
        public int FailCode = -1;
        public Func<bool> CheckCondition;

        public void HandleException<T>(int failCode, T triggerObject, Predicate<T> exceptionTrigger)
        {
            if (exceptionTrigger(triggerObject))
                FailCode = failCode;
        }

        public void HandleNullException(int failCode, object triggerObject)
        {
            if (CheckNull(triggerObject))
                FailCode = failCode;
        }

        public bool CheckNull(object obj)
        {
            return obj == null;
        }
    }
}
