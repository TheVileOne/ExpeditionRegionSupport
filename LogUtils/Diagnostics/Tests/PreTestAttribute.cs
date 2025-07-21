using System;

namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Identifies a method that should be run before testing
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PreTestAttribute : Attribute;
}
