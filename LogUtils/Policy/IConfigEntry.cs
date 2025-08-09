namespace LogUtils.Policy
{
    /// <summary>
    /// Exposes members for setting, or updating a config value
    /// </summary>
    public interface IConfigEntry
    {
        /// <summary>
        /// Assigns value stored in the base config entry in the value cache
        /// </summary>
        void SetValueFromBase();

        /// <summary>
        /// Updates config entry with cached data
        /// </summary>
        void UpdateBaseEntry();
    }

    /// <inheritdoc cref="IConfigEntry"/>
    public interface IConfigEntry<T> : IConfigEntry
    {
        /// <summary>
        /// The current config value
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Assigns a new value to value cache
        /// </summary>
        void SetValue(T newValue);

        /// <summary>
        /// Assigns a new value to value cache without throwing a value changed event
        /// </summary>
        void SetValueSilently(T newValue);
    }
}
