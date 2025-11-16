namespace LogUtils.Enums
{
    /// <summary>
    /// The non-generic representation of a <see cref="SharedExtEnum{T}"/> derived type
    /// </summary>
    public interface IExtEnumBase
    {
        /// <summary>
        /// Index position for this entry in the <see cref="ExtEnumType.entries"/> list
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// This <see cref="ExtEnum{T}"/> entry is associated with a valid (non-negative) <see cref="ExtEnum{T}.Index"/> value
        /// </summary>
        public bool Registered { get; }

        /// <summary>
        /// An identifying string assigned to each <see cref="ExtEnum{T}"/> entry
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Registers the <see cref="ExtEnum{T}"/> entry
        /// </summary>
        /// <remarks>This will register all other instances representing this entry</remarks>
        public void Register();

        /// <summary>
        /// Unregisters the <see cref="ExtEnum{T}"/> entry
        /// </summary>
        /// <remarks>This will unregister all other instances representing this entry</remarks>
        public void Unregister();
    }
}
