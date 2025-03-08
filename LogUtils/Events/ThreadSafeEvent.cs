namespace LogUtils.Events
{
    public class ThreadSafeEvent<TSource, TData>
    {
        private EventHandler<TSource, TData> _handler;

        public event EventHandler<TSource, TData> Handler
        {
            add
            {
                lock (this)
                    _handler += value;
            }
            remove
            {
                lock (this)
                    _handler -= value;
            }
        }

        public ThreadSafeEvent()
        {
        }

        public ThreadSafeEvent(EventHandler<TSource, TData> handler)
        {
            _handler = handler;
        }

        public void Raise(TSource source, TData data)
        {
            lock (this)
                _handler?.Invoke(source, data);
        }
    }
}
