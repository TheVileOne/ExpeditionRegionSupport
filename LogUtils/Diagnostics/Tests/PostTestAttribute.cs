using System;

namespace LogUtils.Diagnostics.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PostTestAttribute : Attribute
    {
    }
}
