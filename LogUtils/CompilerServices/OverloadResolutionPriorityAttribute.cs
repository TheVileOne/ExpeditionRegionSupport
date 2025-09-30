#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices
{
#if !NET9_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class OverloadResolutionPriorityAttribute : Attribute
    {
        public OverloadResolutionPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }
#endif
}
#pragma warning restore IDE0130 // Namespace does not match folder structure
