﻿using Aix.XxlJobExecutor.Utils;
using DotXxlJobExecutor.Foundation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.Foundation
{
    /// <summary>
    /// 单线程任务执行器
    /// </summary>
    public class SingleThreadTaskExecutor : ITaskExecutor
    {
        public static int MaxTaskCount = int.MaxValue;
        IBlockingQueue<IRunnable> _taskQueue = QueueFactory.Instance.CreateBlockingQueue<IRunnable>();
        protected readonly PriorityQueue<IScheduledRunnable> ScheduledTaskQueue = new PriorityQueue<IScheduledRunnable>();
        volatile bool _isStart = false;

        volatile bool _isStartDelay = false;//是否开启延迟任务线程，有插入延迟任务时就 自动开启，默认不开启

        private void StartRunTask()
        {
            Task.Factory.StartNew(async () =>
            {
                while (_isStart)
                {
                    try
                    {
                        var action = _taskQueue.Dequeue();
                        await action.Run(action.state);
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void StartRunDelayTask()
        {
            if (_isStartDelay) return;
            lock (this)
            {
                if (_isStartDelay) return;
                _isStartDelay = true;
            }
            Task.Factory.StartNew(async () =>
            {
                while (_isStart)
                {
                    try
                    {
                        RunDelayTask();
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void RunDelayTask()
        {
            lock (ScheduledTaskQueue)
            {
                IScheduledRunnable nextScheduledTask = this.ScheduledTaskQueue.Peek();
                if (nextScheduledTask != null)
                {
                    var tempDelay = nextScheduledTask.TimeStamp - DateUtils.GetTimeStamp();
                    if (tempDelay > 0)
                    {
                        Monitor.Wait(ScheduledTaskQueue, (int)tempDelay);
                    }
                    else
                    {
                        this.ScheduledTaskQueue.Dequeue();
                        Execute(nextScheduledTask);
                    }
                }
                else
                {
                    Monitor.Wait(ScheduledTaskQueue);
                }
            }

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

        public ITaskExecutor GetSingleThreadTaskExecutor(int routeId)
        {
            return this;
        }
        public void Execute(Func<object, Task> action, object state)
        {
            Execute(new TaskRunnable(action, state));
        }

        public void Execute(IRunnable task)
        {
            if (this._taskQueue.Count > MaxTaskCount) throw new Exception($"即时任务队列超过{MaxTaskCount}条");
            _taskQueue.Enqueue(task);
        }

        public void Schedule(IRunnable action, TimeSpan delay)
        {
            Schedule(new ScheduledRunnable(action, DateUtils.GetTimeStamp(DateTime.Now.Add(delay))));
        }

        public void Schedule(Func<object, Task> action, object state, TimeSpan delay)
        {
            Schedule(new TaskRunnable(action, state), delay);
        }

        private void Schedule(IScheduledRunnable task)
        {
            StartRunDelayTask();

            this.Execute((state) =>
            {
                lock (ScheduledTaskQueue)
                {
                    if (this.ScheduledTaskQueue.Count > MaxTaskCount) throw new Exception($"延迟任务队列超过{MaxTaskCount}条");
                    this.ScheduledTaskQueue.Enqueue(task);
                    Monitor.Pulse(ScheduledTaskQueue);
                }
                return Task.CompletedTask;
            }, null);
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
                //StartRunDelayTask();
            });
        }

        public void Stop()
        {
            //Console.WriteLine("--------------stop SingleThreadTaskExecutor-------------------");
            if (this._isStart == false) return;
            lock (this)
            {
                if (this._isStart)
                {
                    this._isStart = false;
                    this._isStartDelay = false;
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
