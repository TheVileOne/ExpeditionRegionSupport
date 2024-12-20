using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Enums
{
    public class EventTag : SharedExtEnum<EventTag>
    {
        public EventTag(string value, bool register = false) : base(value, register)
        {
        }

        public static readonly EventTag Utility = new EventTag(UtilityConsts.UTILITY_NAME, true);
    }
}
