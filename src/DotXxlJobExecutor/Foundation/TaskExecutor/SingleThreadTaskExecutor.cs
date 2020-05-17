using DotXxlJobExecutor.Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Foundation
{


    /// <summary>
    /// 单线程任务执行器
    /// </summary>
    public class SingleThreadTaskExecutor : ITaskExecutor
    {

        IBlockingQueue<Func<Task>> _taskQueue = QueueFactory.Instance.CreateBlockingQueue<Func<Task>>();
        volatile bool _isStart = false;

        private void StartRunTask()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var action = _taskQueue.Dequeue();
                        await action();
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task handlerException(Exception ex)
        {
            if (OnException != null)
            {
                await OnException(ex);
            }
        }

        #region ITaskExecutor

        public event Func<Exception, Task> OnException;
        public void Execute(Func<Task> action)
        {
            _taskQueue.Enqueue(action);
        }

        public void Start()
        {
            if (_isStart) return;
            lock (this)
            {
                if (_isStart) return;
                _isStart = true;
            }

            Task.Run(() =>
            {
                StartRunTask();
            });
        }

        public void Stop()
        {
            if (this._isStart == false) return;
            lock (this)
            {
                if (this._isStart)
                {
                    this._isStart = false;
                }
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}
