using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Foundation
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    public interface ITaskExecutor : IDisposable
    {
        void Execute(Func<Task> action);

        void Start();

        void Stop();

        /// <summary>
        /// 获取单线程执行器 根据routeId获取固定的执行器 （保证相同的routeId使用同一个线程执行器）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ITaskExecutor GetSingleThreadTaskExecutor(int routeId);

        event Func<Exception, Task> OnException;
    }
}
