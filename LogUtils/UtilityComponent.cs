using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string Tag;
    }
}
