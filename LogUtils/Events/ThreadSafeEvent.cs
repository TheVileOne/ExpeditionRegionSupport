namespace LogUtils.Events
{
    public class ThreadSafeEvent<TSource, TData>
    {
        private EventHandler handler;

        public void Add(EventHandler handler)
        {
            lock (this)
                this.handler += handler;
        }

        public void Remove(EventHandler handler)
        {
            lock (this)
                this.handler -= handler;
        }

        public void Raise(TSource source, TData data)
        {
            lock (this)
                handler?.Invoke(source, data);
        }

        public delegate void EventHandler(TSource sender, TData data);
    }
}
