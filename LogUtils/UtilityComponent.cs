using LogUtils.Compatibility.BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// Base class for UnityEngine components used by the assembly
    /// </summary>
    public abstract class UtilityComponent : MonoBehaviour
    {
        /// <summary>
        /// The version of a component is the same as its assembly version. In the situation of multiple loaded assemblies,
        /// this version may not match other loaded assemblies
        /// </summary>
        public Version Version => UtilityCore.AssemblyVersion;

        /// <summary>
        /// A tag used for identification purposes (Not the same field as <see cref="Component.tag"/>)
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

        internal static T Create<T>() where T : UtilityComponent
        {
            return BepInExInfo.ManagerObject.AddComponent<T>();
        }
    }
}
