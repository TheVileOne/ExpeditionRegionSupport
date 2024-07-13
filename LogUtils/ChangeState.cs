using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public class ChangeState
    {
        public object TargetObject;
        public FieldInfo TargetField;

        /// <summary>
        /// The value of the field when the state was created
        /// </summary>
        public object FieldState;

        public ChangeState(object target, string fieldName)
        {
            TargetObject = target;
            TargetField = TargetObject.GetType().GetField(fieldName);
            FieldState = TargetField.GetValue(TargetObject);
        }

        public void Restore()
        {
            TargetField.SetValue(TargetObject, FieldState);
        }
    }
}
