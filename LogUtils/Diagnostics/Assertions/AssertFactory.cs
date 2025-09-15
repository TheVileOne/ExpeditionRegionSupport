using LogUtils.Enums;
using System.ComponentModel;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        #region Normal assert API

        /// <summary>
        /// Creates a <see cref="Condition{T}"/> for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public static Condition<T> That<T>(T value)
        {
            return Create(value, DebugContext.Normal);
        }

        /// <inheritdoc cref="That{T}(T)"/>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="behavior">Represents options for handling assert behavior</param>
        public static Condition<T> That<T>(T value, AssertBehavior behavior)
        {
            return Create(value, behavior, DebugContext.Normal);
        }

        /// <inheritdoc cref="That{T}(T)"/>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        public static Condition<T> That<T>(T value, IConditionHandler handler)
        {
            return Create(value, handler, DebugContext.Normal);
        }

        /// <inheritdoc cref="That{T}(T)"/>
        public static Condition<T?> That<T>(T? value) where T : struct
        {
            return Create(value, DebugContext.Normal);
        }

        /// <inheritdoc cref="That{T}(T, AssertBehavior)"/>
        public static Condition<T?> That<T>(T? value, AssertBehavior behavior) where T : struct
        {
            return Create(value, behavior, DebugContext.Normal);
        }

        /// <inheritdoc cref="That{T}(T, IConditionHandler)"/>
        public static Condition<T?> That<T>(T? value, IConditionHandler handler) where T : struct
        {
            return Create(value, handler, DebugContext.Normal);
        }
        #endregion
        #region Test assert API

        /// <summary>
        /// Creates a <see cref="Condition{T}"/> for asserting the state of a specified value in a test environment
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T> Test<T>(T value)
        {
            return Create(value, DebugContext.TestCondition);
        }

        /// <inheritdoc cref="Test{T}(T)"/>
        /// <inheritdoc cref="That{T}(T, AssertBehavior)" select="param"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T> Test<T>(T value, AssertBehavior behavior)
        {
            return Create(value, behavior, DebugContext.TestCondition);
        }

        /// <inheritdoc cref="Test{T}(T)"/>
        /// <inheritdoc cref="That{T}(T, IConditionHandler)" select="param"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T> Test<T>(T value, IConditionHandler handler)
        {
            return Create(value, handler, DebugContext.TestCondition);
        }

        /// <inheritdoc cref="Test{T}(T)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T?> Test<T>(T? value) where T : struct
        {
            return Create(value, DebugContext.TestCondition);
        }

        /// <inheritdoc cref="Test{T}(T)"/>
        /// <inheritdoc cref="That{T}(T, AssertBehavior)" select="param"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T?> Test<T>(T? value, AssertBehavior behavior) where T : struct
        {
            return Create(value, behavior, DebugContext.TestCondition);
        }

        /// <inheritdoc cref="Test{T}(T)"/>
        /// <inheritdoc cref="That{T}(T, IConditionHandler)" select="param"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Condition<T?> Test<T>(T? value, IConditionHandler handler) where T : struct
        {
            return Create(value, handler, DebugContext.TestCondition);
        }
        #endregion
        #region Implementation

        internal static Condition<T> Create<T>(T value, DebugContext context)
        {
            Debug.LastKnownContext = context;
            return new Condition<T>(value, AssertHandler.Default);
        }

        internal static Condition<T> Create<T>(T value, AssertBehavior behavior, DebugContext context)
        {
            Debug.LastKnownContext = context;
            return new Condition<T>(value, AssertHandler.GetTemplateWithBehavior(behavior));
        }

        internal static Condition<T> Create<T>(T value, IConditionHandler handler, DebugContext context)
        {
            Debug.LastKnownContext = context;
            return new Condition<T>(value, handler);
        }

        internal static Condition<T?> Create<T>(T? value, DebugContext context) where T : struct
        {
            Debug.LastKnownContext = context;
            return new Condition<T?>(value, AssertHandler.Default);
        }

        internal static Condition<T?> Create<T>(T? value, AssertBehavior behavior, DebugContext context) where T : struct
        {
            Debug.LastKnownContext = context;
            return new Condition<T?>(value, AssertHandler.GetTemplateWithBehavior(behavior));
        }

        internal static Condition<T?> Create<T>(T? value, IConditionHandler handler, DebugContext context) where T : struct
        {
            Debug.LastKnownContext = context;
            return new Condition<T?>(value, handler);
        }
        #endregion
    }
}
