using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class ScopeProvider
    {
        public event Action<object> OnEnter, OnExit;

        internal Dictionary<object, Scope> AvailableScopes = new Dictionary<object, Scope>();

        /// <summary>
        /// Is there at least one scope currently being occupied?
        /// </summary>
        public bool Entered
        {
            get
            {
                Scope[] scopes = AvailableScopes.Values.ToArray();
                return Array.Exists(scopes, scope => scope.EnterCount > 0);
            }
        }

        /// <summary>
        /// When true, each scope context will track its number of entries
        /// </summary>
        internal bool IsReentrant;

        public Scope this[object context]
        {
            get
            {
                if (AvailableScopes.TryGetValue(context, out Scope scope))
                    return scope;
                return null;
            }
        }

        public ScopeProvider(bool reentrant)
        {
            IsReentrant = reentrant;
        }

        public IDisposable Enter(object context)
        {
            Scope fromContext = this[context];

            //UtilityLogger.DebugLog("Entering context " + context);
            if (fromContext == null)
            {
                fromContext = new Scope(this, context);
                AvailableScopes[context] = fromContext;
            }

            fromContext.Enter();
            return fromContext;
        }

        public IDisposable Exit(object context)
        {
            Scope fromContext = this[context];

            if (fromContext == null)
                return null;

            fromContext.Exit();
            return fromContext;
        }

        public class Scope : IDisposable
        {
            /// <summary>
            /// The manager object of the scope
            /// </summary>
            public ScopeProvider Provider;

            /// <summary>
            /// Used to attach identifying information to the object
            /// </summary>
            public object Context;

            /// <summary>
            /// The amount of times this scope has been entered by a specific context that are still active
            /// </summary>
            public int EnterCount { get; private set; }

            public Scope(ScopeProvider provider, object context)
            {
                Provider = provider;
                Context = context;
            }

            public void Enter()
            {
                if (!Provider.IsReentrant && EnterCount > 0) return;

                //TODO: Not thread safe
                EnterCount++;
                Provider.OnEnter?.Invoke(Context);
            }

            public void Exit()
            {
                if (EnterCount == 0) return;

                //TODO: Not thread safe
                EnterCount--;
                Provider.OnExit?.Invoke(Context);
            }

            void IDisposable.Dispose()
            {
                Exit();
            }
        }
    }
}
