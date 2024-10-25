using System;
using System.Reflection;

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

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            Type type = TargetObject.GetType();

            //Checks for the first field match from all inherited types, starting with the most specific type
            do
            {
                TargetField = Array.Find(type.GetFields(flags), field => field.Name == fieldName);

                if (TargetField != null)
                    break;
                type = type.BaseType;
            }
            while (type.BaseType != null);

            if (TargetField == null)
                throw new InvalidOperationException("Field does not exist");

            FieldState = TargetField.GetValue(TargetObject);
        }

        public void Restore()
        {
            TargetField.SetValue(TargetObject, FieldState);
        }
    }
}
