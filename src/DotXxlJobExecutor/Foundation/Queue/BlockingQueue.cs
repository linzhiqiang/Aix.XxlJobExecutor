using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.Foundation
{
    internal class BlockingQueue<T> : IBlockingQueue<T>
    {
        private BlockingCollection<T> BlockQueue;

        public BlockingQueue()
        {
            BlockQueue = new BlockingCollection<T>(new ConcurrentQueue<T>()); 
        }
        public int Count
        {
            get
            {
                return BlockQueue.Count;
            }
        }

        public void Enqueue(T item)
        {
            BlockQueue.Add(item);
        }
        public T Dequeue()
        {
            return BlockQueue.Take();
        }

        public bool TryDequeue(out T item)
        {
            return BlockQueue.TryTake(out item);
        }
    }
}
