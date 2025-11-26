using LogUtils.Enums;
using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tests.Components
{
    /// <summary>
    /// Factory implementation
    /// </summary>
    public sealed class TestEnumFactory
    {
        /// <summary>
        /// Objects that have been created by the factory that have yet to be cleaned up
        /// </summary>
        internal static List<IExtEnumBase> Objects = new List<IExtEnumBase>();

        /// <summary>
        /// Creates a new <see cref="TestEnum"/> instance
        /// </summary>
        public TestEnum Create(string value, bool register = false)
        {
            return Add(new TestEnum(value, register));
        }

        /// <summary>
        /// Creates a new <see cref="TestEnum"/> instance using the <see cref="SharedExtEnum{T}.Value"/> of an existing instance
        /// </summary>
        public TestEnum FromTarget(TestEnum target)
        {
            return Add(new TestEnum(target.Value, false));
        }

        /// <summary>
        /// Notify the factory class that an object should be managed
        /// </summary>
        internal static T Add<T>(T newObject) where T : IExtEnumBase
        {
            Objects.Add(newObject);
            return newObject;
        }

        /// <summary>
        /// Ensure that <see cref="ExtEnum{T}"/> entries used for testing are properly disposed, and will not affect runtime behavior
        /// </summary>
        public static void DisposeObjects()
        {
            foreach (IExtEnumBase extEnum in Objects)
            {
                UtilityLogger.DebugLog("Disposing " + extEnum.Value);
                if (extEnum.Registered)
                    extEnum.Unregister();

                if (extEnum is IShareable sharedEntry) //All factory implementations create SharedExtEnum derived types - this should always be true
                {
                    //Clear reference to this instance stored in the shared data register
                    Type extEnumType = extEnum.GetType();
                    UtilityCore.DataHandler.DataCollection[extEnumType].Remove(sharedEntry);
                }
            }
            Objects.Clear();
        }
    }
}
