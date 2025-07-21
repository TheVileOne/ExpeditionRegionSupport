namespace LogUtils.Properties
{
    public struct MultiValueProperty<T>
    {
        public LogProperties Owner;
        public bool ReadOnly => Owner.ReadOnly;

        private T _value;

        public T Value
        {
            get => ValueTemp ?? _value;
            set
            {
                if (ReadOnly) return;
                _value = value;
            }
        }

#nullable enable
        public T? ValueTemp;
#nullable disable

        public MultiValueProperty(LogProperties owner, T value = default)
        {
            Owner = owner;
            Value = value;
        }
    }
}
