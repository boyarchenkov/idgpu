using System;
using System.Collections.Generic;
using System.Text;

namespace M.Tools
{
    public class FixedQueue<T> : IEnumerable<T>
    {
        public int Count
        {
            get
            {
                return q.Count;
            }
        }
        public T[] Buffer
        {
            get
            {
                if (buffer == null) buffer = q.ToArray();
                return buffer;
            }
        }
        public T this[int index]
        {
            get
            {
                return Buffer[index];
            }
        }

        public FixedQueue()
        {
            q = new Queue<T>();
            buffer = null;
        }

        public void Clear()
        {
            q.Clear();
            buffer = null;
        }
        public void Enqueue(T item)
        {
            buffer = null;
            q.Enqueue(item);
        }
        public T Peek()
        {
            return q.Peek();
        }
        public T Dequeue()
        {
            return q.Dequeue();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return q.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (q as System.Collections.IEnumerable).GetEnumerator();
        }

        private T[] buffer;
        private Queue<T> q;
    }
}
