using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    internal static class Assert
    {
        internal static void OnResult(List<IConditionHandler> handlers, ConditionResults result)
        {
            foreach (var handler in handlers)
                handler.Handle(result);
        }
    }
}
