using Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Collections
{
    public class DialogCollection<T> : IEnumerable<T> where T : Dialog
    {
        /// <summary>
        /// The <see cref="ProcessManager"/> instance responsible for managing Rain World dialogs
        /// </summary>
        public ProcessManager Manager { get; }

        public DialogCollection(ProcessManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            Manager = manager;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> dialogs = Manager.dialogStack.OfType<T>();

            T activeDialog = Manager.dialog as T;
            if (activeDialog != null)
                dialogs = dialogs.Append(activeDialog);

            return (IEnumerator<T>)dialogs.Reverse(); //Order by active priority
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
