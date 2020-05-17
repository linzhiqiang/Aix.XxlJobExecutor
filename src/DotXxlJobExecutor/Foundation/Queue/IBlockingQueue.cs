using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.Foundation
{
    public interface IBlockingQueue<T>
    {
        int Count { get; }

        void Enqueue(T item);

        /// <summary>
        /// 阻塞
        /// </summary>
        /// <returns></returns>
        T Dequeue();

        bool TryDequeue(out T item);

    }
}
