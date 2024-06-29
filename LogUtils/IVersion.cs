using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public interface IVersion
    {
        /// <summary>
        /// The version associated with an implementation of a class instance (typically associated with a particular release of the containing assembly)
        /// </summary>
        Version Version { get; }
    }
}
