namespace LogUtils
{
    public interface ISelectable
    {
        /// <summary>
        /// Selects the first element
        /// </summary>
        public void SelectFirst();

        /// <summary>
        /// Selects the last element
        /// </summary>
        public void SelectLast();

        /// <summary>
        /// Selects the previous element
        /// </summary>
        public void SelectPrev();

        /// <summary>
        /// Selects the next element
        /// </summary>
        public void SelectNext();
    }
}
