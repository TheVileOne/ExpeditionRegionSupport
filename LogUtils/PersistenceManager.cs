using System;
using System.Collections.Generic;

namespace LogUtils
{
    public class PersistenceManager : UtilityComponent
    {
        internal List<WeakReference<PersistentFileHandle>> References = new List<WeakReference<PersistentFileHandle>>();

        public override string Tag => UtilityConsts.ComponentTags.PERSISTENCE_MANAGER;

        public PersistenceManager()
        {
            enabled = true;
        }

        public void Update()
        {
            bool hasDisposedReferences = false;
            foreach (var reference in References)
            {
                if (reference.TryGetTarget(out PersistentFileHandle handle))
                {
                    handle.UpdateLifetime();

                    if (!handle.IsAlive)
                    {
                        handle.Dispose();
                        hasDisposedReferences = true;
                    }
                }
                else
                    hasDisposedReferences = true;
            }

            if (hasDisposedReferences)
                References.RemoveAll(r => !r.TryGetTarget(out PersistentFileHandle handle) || !handle.IsAlive);
        }
    }
}
