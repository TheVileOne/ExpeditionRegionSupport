using System;
using System.Linq;

namespace LogUtils
{
    public class PersistenceManager : UtilityComponent
    {
        internal WeakReferenceCollection<PersistentFileHandle> References = new WeakReferenceCollection<PersistentFileHandle>();

        public override string Tag => UtilityConsts.ComponentTags.PERSISTENCE_MANAGER;

        /// <summary>
        /// Called whenever a PersistentFileHandle is disposed
        /// </summary>
        public event Action<PersistentFileHandle> OnHandleDisposed;

        public PersistenceManager()
        {
            enabled = true;
        }

        public void Update()
        {
            bool hasDisposedReferences = false;
            foreach (PersistentFileHandle handle in References.Where(handleIsInvalid))
            {
                handle.Dispose();
                hasDisposedReferences = true;
            }

            if (hasDisposedReferences)
                References.RemoveAll(handleIsInvalid);

            static bool handleIsInvalid(PersistentFileHandle handle)
            {
                return !handle.IsAlive;
            }
        }

        internal void NotifyOnDispose(PersistentFileHandle handle)
        {
            OnHandleDisposed?.Invoke(handle);
        }
    }
}
