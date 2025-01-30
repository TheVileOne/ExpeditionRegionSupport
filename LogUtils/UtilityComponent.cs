using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    public abstract class UtilityComponent : MonoBehaviour
    {
        /// <summary>
        /// The version of a component is the same as its assembly version. In the situation of multiple loaded assemblies,
        /// this version may not match other loaded assemblies
        /// </summary>
        public Version Version => UtilityCore.AssemblyVersion;

        /// <summary>
        /// A tag used for identification purposes (Not the same field as Component.tag)
        /// </summary>
        public abstract string Tag { get; }

        /// <summary>
        /// Returns field values stored by the component using the field name as the key 
        /// </summary>
        public virtual Dictionary<string, object> GetFields()
        {
            Dictionary<string, object> fieldDictionary = new Dictionary<string, object>();

            fieldDictionary[nameof(Tag)] = Tag;
            fieldDictionary[nameof(Version)] = Version;
            return fieldDictionary;
        }
    }
}
