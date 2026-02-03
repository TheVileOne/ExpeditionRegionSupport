using LogUtils.Collections;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class PersistenceManager : UtilityComponent
    {
        internal WeakReferenceCollection<PersistentFileHandle> References = new WeakReferenceCollection<PersistentFileHandle>();

        /// <inheritdoc/>
        public override string Tag => UtilityConsts.ComponentTags.PERSISTENCE_MANAGER;

        /// <summary>
        /// Invoked when a <see cref="PersistentFileHandle"/> instance is disposed
        /// </summary>
        public event Action<PersistentFileHandle> OnHandleDisposed;

        public PersistenceManager()
        {
            enabled = true;
        }

        /// <summary>
        /// Required method for <see cref="MonoBehaviour"/> update process
        /// </summary>
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
