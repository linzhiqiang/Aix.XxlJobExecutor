﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.Foundation
{
    /// <summary>
    /// 多线程任务执行器
    /// </summary>
    public class MultithreadTaskExecutor : ITaskExecutor
    {
        static readonly int DefaultTaskExecutorThreadCount = Environment.ProcessorCount * 2;//默认线程数
        static Func<ITaskExecutor> DefaultExecutorFactory = () => new SingleThreadTaskExecutor();
        readonly ITaskExecutor[] EventLoops;
        int requestId;

        public MultithreadTaskExecutor() : this(DefaultTaskExecutorThreadCount)
        {

        }
        public MultithreadTaskExecutor(int threadCount)
        {
            threadCount = threadCount > 0 ? threadCount : DefaultTaskExecutorThreadCount;
            this.EventLoops = new ITaskExecutor[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var eventLoop = DefaultExecutorFactory();
                this.EventLoops[i] = eventLoop;
                eventLoop.OnException += EventLoop_OnException;
            }
        }

        public ITaskExecutor GetNext()
        {
            int id = Interlocked.Increment(ref this.requestId);
            return GetNext(id);
        }

        public ITaskExecutor GetNext(int index)
        {
            return this.EventLoops[Math.Abs(index % this.EventLoops.Length)];
        }

        public ITaskExecutor GetSingleThreadTaskExecutor(int routeId)
        {
            return GetNext(routeId);
        }


        private async Task EventLoop_OnException(Exception ex)
        {
            if (OnException != null) await OnException(ex);
        }

        #region ITaskExecutor

        public event Func<Exception, Task> OnException;

        public void Execute(Func<object,Task> action, object state)
        {
            this.GetNext().Execute(action, state);
        }

        public void Execute(IRunnable task)
        {
            this.GetNext().Execute(task);
        }

        public void Schedule(IRunnable action, TimeSpan delay)
        {
            this.GetNext().Schedule(action, delay);
        }

        public void Schedule(Func<object,Task> action, object state,TimeSpan delay)
        {
            this.GetNext().Schedule(action, state, delay);
        }
        public void Start()
        {
            foreach (var item in this.EventLoops)
            {
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in this.EventLoops)
            {
                item.Stop();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
        #endregion

    }
}
