using System;

namespace LogUtils.Properties.Formatting
{
    /// <summary>
    /// An attribute that contains data necessary for LogUtils to construct a LogRule instance.
    /// </summary>
    /// <remarks>Methods that use this attribute must have the same signature as <see cref="LogRule.ApplyDelegate"/>.</remarks>
    /// <param name="RuleName">The name of the LogRule represented by this attribute</param>
    /// <param name="EnabledByDefault">Indicates that the LogRule should start in an active, or inactive state</param>
    /// <param name="Priority">The process order priority of the LogRule</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LogRuleAttribute(string RuleName, bool EnabledByDefault = true, float Priority = 0.75f) : Attribute
    {
        /// <summary>
        /// The name of the LogRule represented by this attribute
        /// </summary>
        public string RuleName = RuleName;

        /// <summary>
        /// Indicates that the LogRule should start in an active, or inactive state
        /// </summary>
        public bool EnabledByDefault = EnabledByDefault;

        /// <summary>
        /// The process order priority of the LogRule
        /// </summary>
        public float Priority = Priority;
    }

    /// <summary>
    /// Indicate that assembly contains LogRule definitionx. LogUtils will attempt to activate any reflection loaded LogRules from your assembly.
    /// </summary>
    /// <param name="TypeHints">The array of types containing all reflection loaded LogRule members. When left empty, all assembly types will be checked.</param>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class LogRuleAssemblyAttribute(params Type[] TypeHints) : Attribute
    {
        internal Type[] Types = TypeHints;
    }

    /// <summary>
    /// Indicate that LogUtils should try to activate a LogRule with this attribute through reflection
    /// </summary>
    /// <remarks>
    /// <para>Incorrectly configured attributes may throw a <see cref="MissingAttributeException"/>.</para>
    /// <para>Usage:</para>
    /// <para>Classes that use this attribute must inherit from <see cref="LogRule"/>.</para>
    /// <para>Methods that use this attribute must have the same signature as <see cref="LogRule.ApplyDelegate"/>.</para>
    /// <para>Methods that use this attribute must also have the <see cref="LogRuleAttribute"/>.</para>
    /// <para>Attribute does nothing without applying the <see cref="LogRuleAssemblyAttribute"/> to the executing assembly.</para></remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ReflectionLoadedAttribute : Attribute;
}
