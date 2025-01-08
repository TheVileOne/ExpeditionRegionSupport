namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public static Condition<T> That<T>(T value)
        {
            return new Condition<T>(value, AssertHandler.CurrentTemplate);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="behavior">Represents options for handling assert behavior</param>
        public static Condition<T> That<T>(T value, AssertBehavior behavior)
        {
            AssertHandler handler = AssertHandler.CurrentTemplate as AssertHandler;

            //In order to apply the AssertBehavior, we must be using an instance type that can handle it
            if (handler == null)
                handler = AssertHandler.DefaultHandler;

            bool isNewBehavior = behavior != handler.Behavior;

            //On new behaviors, we must clone the existing handler to ensure that this behavior only applies to a single assert chain
            if (isNewBehavior)
                handler = handler.Clone(behavior);

            return new Condition<T>(value, handler);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        public static Condition<T> That<T>(T value, IConditionHandler handler)
        {
            return new Condition<T>(value, handler);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public static Condition<T?> That<T>(T? value) where T : struct
        {
            return new Condition<T?>(value, AssertHandler.CurrentTemplate);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="behavior">Represents options for handling assert behavior</param>
        public static Condition<T?> That<T>(T? value, AssertBehavior behavior) where T : struct
        {
            AssertHandler handler = AssertHandler.CurrentTemplate as AssertHandler;

            //In order to apply the AssertBehavior, we must be using an instance type that can handle it
            if (handler == null)
                handler = AssertHandler.DefaultHandler;

            bool isNewBehavior = behavior != handler.Behavior;

            //On new behaviors, we must clone the existing handler to ensure that this behavior only applies to a single assert chain
            if (isNewBehavior)
                handler = handler.Clone(behavior);

            return new Condition<T?>(value, handler);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        public static Condition<T?> That<T>(T? value, IConditionHandler handler) where T : struct
        {
            return new Condition<T?>(value, handler);
        }
    }
}
