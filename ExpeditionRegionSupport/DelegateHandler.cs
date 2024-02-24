using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    /// <summary>
    /// A class designed for handling Delegate content to perform specific tasks 
    /// </summary>
    public class DelegateHandler
    {
        public static bool InvokeSafely(Delegate del, params object[] args)
        {
            try
            {
                del.DynamicInvoke(args);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger.Log(ex);
                return false;
            }
        }

        public static rtnType InvokeSafely<rtnType>(Delegate del, params object[] args)
        {
            try
            {
                return (rtnType)del.DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Log(ex);
                return default;
            }
        }
    }

    /// <summary>
    /// A DelegateHandler that ensures that a delegate is only invoked once
    /// </summary>
    public class InvokeOnceHandler : DelegateHandler
    {
        /// <summary>
        /// The contents of this field will be invoked only once
        /// </summary>
        public readonly Delegate Target;

        public bool Handled { get; private set; }

        public InvokeOnceHandler(Delegate target)
        {
            Target = target;
        }

        public void InvokeAction()
        {
            if (Handled || Target == null) return;

            Target.DynamicInvoke();
            Handled = true;
        }

        public void InvokeAction<T>(T arg)
        {
            if (Handled || Target == null) return;

            Target.DynamicInvoke(arg);
            Handled = true;
        }

        public void Invoke(params object[] args)
        {
            if (Handled || Target == null) return;

            try
            {
                Target.DynamicInvoke(args);
            }
            finally
            {
                Handled = true;
            }
        }

        public rtnType Invoke<rtnType>(params object[] args)
        {
            if (Handled || Target == null) return default;

            try
            {
                return (rtnType)Target.DynamicInvoke(args);
            }
            finally
            {
                Handled = true;
            }
        }

        public void Reset()
        {
            Handled = false;
        }

        public static IEnumerable<MethodInfo> GetHandlers(MethodInfo[] handlerMethods, string handlerName)
        {
            return handlerMethods.Where(m => m.Name == handlerName);
        }

        /*
        public MethodInfo GetActionSignature<T>(T arg)
        {
        }
        */
            /*
        public static MethodInfo GetActionSignature(bool hasArgs)
        {
            MethodInfo[] handlerMethods = typeof(InvokeOnceHandler).GetMethods();

            IEnumerable<MethodInfo> actionHandlers = GetHandlers(handlerMethods, "InvokeAction");

            var enumerator = actionHandlers.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int paramAmount = enumerator.Current.GetParameters().Length;

                if (hasArgs)
                {
                    if (paramAmount > 0)
                        return enumerator.Current;
                }
                else
                {
                    if (paramAmount == 0)
                        return enumerator.Current;
                }
            }

            return null; //Shouldn't run
            
            MethodInfo info = .Where(m => m.Name == "Invoke" && m.GetParameters().Length!= typeof(void) == hasReturn).First();

            Plugin.Logger.LogInfo(info);
            Plugin.Logger.LogInfo(info.Name);
            Plugin.Logger.LogInfo(info.IsGenericMethod);

            return info;
        }*/

        public static MethodInfo GetInvokeSignature(bool hasReturn)
        {
            MethodInfo info = typeof(InvokeOnceHandler).GetMethods().Where(m => m.Name == "Invoke" && m.ReturnType != typeof(void) == hasReturn).First();

            Plugin.Logger.LogInfo(info);
            Plugin.Logger.LogInfo(info.Name);
            Plugin.Logger.LogInfo(info.IsGenericMethod);

            return info;
        }
    }
}
