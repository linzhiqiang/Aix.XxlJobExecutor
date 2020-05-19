using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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

        /// <summary>
        /// 立即返回
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryDequeue(out T item);

        bool TryDequeue(out T item, int millisecondsTimeout);

        bool TryDequeue(out T item, int millisecondsTimeout, CancellationToken cancellationToken);

    }
}
