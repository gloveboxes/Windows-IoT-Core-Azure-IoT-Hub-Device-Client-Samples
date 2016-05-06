using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IotServices
{
    public class ItemEventArgs<T> : EventArgs
    {
        public ItemEventArgs(T item) { Item = item; }
        public T Item { get; protected set; }
    }


    public class ObservableQueue<T>
    {
        ManualResetEvent dequeueEvent = new ManualResetEvent(false);


        public event EventHandler<ItemEventArgs<T>> Enqueued;
        public event EventHandler<ItemEventArgs<T>> Dequeued;
        public int Count => queue.Count; // { get { return queue.Count; } }

        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public ObservableQueue() {
            Task.Run(new Action (Pump));
        }

        public virtual void Enqueue(T item) {
            queue.Enqueue(item);
            Enqueued?.Invoke(this, new ItemEventArgs<T>(item));
            dequeueEvent.Set();
        }

        public virtual void Dequeue() {
            T item;
            while (!queue.IsEmpty) {
                if (!queue.TryDequeue(out item)) { continue; }
                Dequeued?.Invoke(this, new ItemEventArgs<T>(item));
                break;
            }
        }

        void Pump() {
            while (true) {
                dequeueEvent.WaitOne();
           //     Debug.WriteLine($"Dequeued, Queue Length {Count}");
                if (Count > 0) {
                    Dequeue();
                }
                else {
              //      Debug.WriteLine("Sleep");
                    dequeueEvent.Reset();
                }
            }
        }
    }
}
