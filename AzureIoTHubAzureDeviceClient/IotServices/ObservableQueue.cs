using System;
using System.Collections.Generic;

namespace IotServices
{
    public class ItemEventArgs<T> : EventArgs
    {
        public ItemEventArgs(T item) { Item = item; }
        public T Item { get; protected set; }
    }


    public class ObservableQueue<T>
    {
        public event EventHandler<ItemEventArgs<T>> Enqueued;
        public event EventHandler<ItemEventArgs<T>> Dequeued;
        public int Count => queue.Count; // { get { return queue.Count; } }

        private readonly Queue<T> queue = new Queue<T>();

        public virtual void Enqueue(T item) {
            queue.Enqueue(item);
            Enqueued?.Invoke(this, new ItemEventArgs<T>(item));
        }

        public virtual T Dequeue() {
            var item = queue.Dequeue();
            Dequeued?.Invoke(this, new ItemEventArgs<T>(item));
            return item;
        }
    }
}
