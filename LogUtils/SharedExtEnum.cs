using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public class SharedExtEnum<T> : ExtEnum<T>, IShareable where T : class, IShareable
    {
        public bool Registered => index >= 0;
        public string Tag => value;

        public T ManagedReference;

        public SharedExtEnum(string value, bool register = false) : base(value, register)
        {
            ManagedReference = (T)UtilityCore.DataHandler.GetOrAssign(this);
        }

        public virtual bool CheckTag(string tag)
        {
            return value == tag;
        }
    }
}
