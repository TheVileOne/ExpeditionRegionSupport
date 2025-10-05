#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices
{
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Backported attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerAttribute : Attribute;
#endif
}
#pragma warning restore IDE0130 // Namespace does not match folder structure
