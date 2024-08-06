using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public class SharedExtEnum<T> : ExtEnum<T>, IShareable where T : SharedExtEnum<T>, IShareable
    {
        public override int Index
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Index;
                return index;
            }
        }

        /// <summary>
        /// A null-safe reference to the SharedExtEnum that any mod can access
        /// </summary>
        public T ManagedReference;
        public bool Registered => Index >= 0;
        public string Tag
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Tag;
                return value;
            }
        }

        /// <summary>
        /// An identifying string assigned to each ExtEnum
        /// </summary>
        public new string value
        {
            get => base.value;
            protected set => base.value = value;
        }

        public SharedExtEnum(string value, bool register = false) : base(value, false)
        {
            ManagedReference = (T)UtilityCore.DataHandler.GetOrAssign(this);

            if (register)
            {
                Register();
            }
            else if (!ReferenceEquals(ManagedReference, this) && ManagedReference.Registered) //Propagate field values from registered reference
            {
                base.value = ManagedReference.value;
                valueHash = ManagedReference.valueHash;
                index = ManagedReference.Index;
            }
        }

        public virtual void Register()
        {
            //The shared reference may already exist. Sync any value differences between the two references
            if (!ReferenceEquals(ManagedReference, this))
            {
                if (ManagedReference.Registered)
                {
                    value = ManagedReference.value;
                    valueHash = ManagedReference.valueHash;
                    index = ManagedReference.Index;
                }
                else //Registration status should propagate to managed reference
                {
                    ManagedReference.value = value;
                    ManagedReference.valueHash = valueHash;

                    //Register the managed reference and get the assigned index from it
                    ManagedReference.Register();

                    index = ManagedReference.Index;
                }
            }
            else if (!Registered)
            {
                values.AddEntry(value);
                index = values.Count - 1;
            }
        }

        public virtual bool CheckTag(string tag)
        {
            return string.Equals(value, tag, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
