using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.HookUtils
{
    public delegate void HookDelegate(ILCursor cursor);
    public class ILWrapper
    {
        public HookDelegate RunBefore;
        public HookDelegate RunAfter;

        /// <summary>
        /// Creates a wrapper allowing IL to be emitted before, and after one, or more IL instructions
        /// </summary>
        /// <param name="task">The payload to run BEFORE and AFTER the instruction at the ILCursor</param>
        public ILWrapper(HookDelegate task) : this(task, task)
        {
        }

        /// <summary>
        /// Creates a wrapper allowing IL to be emitted before, and after one, or more IL instructions
        /// </summary>
        /// <param name="taskBefore">The payload to run BEFORE the instruction at the ILCursor</param>
        /// <param name="taskAfter">The payload to run AFTER the instruction at the ILCursor</param>
        public ILWrapper(HookDelegate taskBefore, HookDelegate taskAfter)
        {
            RunBefore = taskBefore;
            RunAfter = taskAfter;
        }

        /// <summary>
        /// Handle tasks BEFORE and AFTER the instruction at the given ILCursor
        /// </summary>
        /// <param name="cursor">The ILCursor to be handled</param>
        public void Apply(ILCursor cursor)
        {
            RunBefore(cursor);
            cursor.Index++; //TODO: Implement multi-instruction support
            RunAfter(cursor);
        }
    }
}
