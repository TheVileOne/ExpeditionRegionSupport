using BepInEx.Configuration;

namespace LogUtils.Policy
{
    /// <summary>
    /// ConfigEntry wrapper class
    /// </summary>
    /// <remarks>Provides an extra class controlled place to store data values that wont be accessed, or modified upon a config save, or reload operation</remarks>
    public class CachedConfigEntry<T> : ConfigEntryBase, IConfigEntry<T>
    {
        internal readonly ConfigEntry<T> BaseEntry;

        /// <summary>
        /// Event invoked when value has changed from its last set value
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;


        /// <inheritdoc/>
        public T Value { get; private set; }

        /// <inheritdoc/>
        public override object BoxedValue
        {
            get => Value;
            set => Value = (T)value;
        }

        /// <summary>
        /// Creates a new <see cref="CachedConfigEntry{T}"/> instance
        /// </summary>
        /// <param name="baseEntry">The config entry to wrap around</param>
        public CachedConfigEntry(ConfigEntry<T> baseEntry) : base(baseEntry.ConfigFile, baseEntry.Definition, baseEntry.SettingType, baseEntry.DefaultValue, baseEntry.Description)
        {
            BaseEntry = baseEntry;
            SetValueFromBase();
        }

        /// <inheritdoc/>
        public void SetValue(T newValue)
        {
            T oldValue = Value;
            bool valueChanged = !Equals(oldValue, newValue);

            Value = newValue;
            if (valueChanged)
                ValueChanged?.Invoke(this, oldValue);
        }

        /// <inheritdoc/>
        public void SetValueFromBase()
        {
            SetValue(BaseEntry.Value);
        }

        /// <inheritdoc/>
        public void SetValueSilently(T newValue)
        {
            Value = newValue;
        }

        /// <inheritdoc/>
        public void UpdateBaseEntry()
        {
            BaseEntry.Value = Value;
        }

        /// <summary>
        /// Method signature for ValueChanged event
        /// </summary>
        /// <param name="entry">The changed entry</param>
        /// <param name="oldValue">The last value of the entry</param>
        public delegate void ValueChangedEventHandler(CachedConfigEntry<T> entry, T oldValue);
    }
}
