using System;

namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Identifies a method that should be run after testing
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PostTestAttribute : Attribute;
}
