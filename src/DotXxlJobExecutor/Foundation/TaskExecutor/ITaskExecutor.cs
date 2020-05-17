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

        event Func<Exception, Task> OnException;
    }
}
