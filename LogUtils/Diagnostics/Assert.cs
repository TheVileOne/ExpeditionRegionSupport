using LogUtils.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Diagnostics
{
    internal static class Assert
    {
        internal static void OnResult(IConditionHandler handler, ConditionResults result)
        {
            if (handler != null)
            {
                if (handler.AcceptsCallerOnCondition(result.HasPassed))
                    handler.Caller = AssemblyUtils.GetCallingAssembly();

                handler.Handle(result);
            }
        }
    }
}
