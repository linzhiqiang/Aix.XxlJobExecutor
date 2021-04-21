using Aix.XxlJobExecutor.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.Foundation
{
    /// <summary>
    /// 队列工厂
    /// </summary>
    public class QueueFactory
    {
        public static QueueFactory Instance = new QueueFactory();

        private QueueFactory() { }

        /// <summary>
        /// 创建阻塞队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IBlockingQueue<T> CreateBlockingQueue<T>()
        {
            return new BlockingQueue<T>();
        }
    }
}
