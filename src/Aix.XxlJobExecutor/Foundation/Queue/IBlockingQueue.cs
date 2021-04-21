using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.XxlJobExecutor.Foundation
{
    /// <summary>
    /// 阻塞队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBlockingQueue<T>
    {
        int Count { get; }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);

        /// <summary>
        /// 阻塞 一直到有元素
        /// </summary>
        /// <returns></returns>
        T Dequeue();

        /// <summary>
        /// 立即返回
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryDequeue(out T item);

        /// <summary>
        /// 没有元素时阻塞millisecondsTimeout时间
        /// </summary>
        /// <param name="item"></param>
        /// <param name="millisecondsTimeout">阻塞时间</param>
        /// <returns></returns>
        bool TryDequeue(out T item, int millisecondsTimeout);

        /// <summary>
        /// 没有元素时阻塞millisecondsTimeout时间
        /// </summary>
        /// <param name="item"></param>
        /// <param name="millisecondsTimeout">阻塞时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        bool TryDequeue(out T item, int millisecondsTimeout, CancellationToken cancellationToken);

    }
}
