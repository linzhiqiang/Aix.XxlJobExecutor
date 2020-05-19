using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Foundation;
using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotXxlJobExecutor.Executor
{
    public class SingleJobExecutor
    {
        private IServiceProvider _serviceProvider;
        private ILogger<SingleJobExecutor> _logger;
        private int _jobId;
        private IJobHandler _jobHandler;
        IBlockingQueue<JobRunRequest> _taskQueue = QueueFactory.Instance.CreateBlockingQueue<JobRunRequest>();
        volatile bool _running = false;
        CancellationTokenSource _cancellationTokenSource;
        private int WattingTime = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
        XxlJobExecutorService _xxlJobExecutorService;
        public SingleJobExecutor(int jobId, IJobHandler handler, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _jobId = jobId;
            _jobHandler = handler;
            _xxlJobExecutorService = _serviceProvider.GetService<XxlJobExecutorService>();
            _logger = _serviceProvider.GetService<ILogger<SingleJobExecutor>>();
        }

        public void Start()
        {
            if (_cancellationTokenSource != null) return;
            lock (this)
            {
                if (_cancellationTokenSource != null) return;
                _cancellationTokenSource = new CancellationTokenSource();
            }
            //启动执行任务
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _running = false;
                    JobRunRequest jobInfo = null;
                    try
                    {
                        //_taskQueue.TryDequeue(out jobInfo, WattingTime, _cancellationTokenSource.Token);
                        //_taskQueue.TryDequeue(out jobInfo);
                        jobInfo = _taskQueue.Dequeue();
                        if (jobInfo == null) break;
                        _running = true;
                        await ExecuteJob(jobInfo);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("程序关闭");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"xxljob执行任务错误:jobId={jobInfo?.jobId},logId={jobInfo?.logId}");
                    }
                    finally
                    {

                    }
                }
                //try
                //{
                //    //_serviceProvider.GetService<XxlJobExecutor>().RemoveJobExecutor(_jobId);
                //    this.Stop();
                //}
                //catch (Exception)
                //{

                //}

            }, _cancellationTokenSource.Token);
        }

        private async Task ExecuteJob(JobRunRequest jobInfo)
        {
            var exeResult = await _jobHandler.Execute(new JobExecuteContext(jobInfo.executorParams));
            await _xxlJobExecutorService.CallBack(jobInfo.logId, exeResult);
        }

        public void Stop()
        {
            if (this._cancellationTokenSource == null) return;
            lock (this)
            {
                if (this._cancellationTokenSource == null) return;
                _cancellationTokenSource.Cancel();

                while (_taskQueue.Count > 0)
                {
                    _taskQueue.TryDequeue(out JobRunRequest jobRunRequest);
                }

                _cancellationTokenSource.Dispose();
                this._cancellationTokenSource = null;
            }
            //其他处理
        }

        public void PushJob(JobRunRequest runRequest)
        {
            _taskQueue.Enqueue(runRequest);
            this.Start();
        }

        public IJobHandler GetJobHandler()
        {
            return _jobHandler;
        }

        public void ChangeJobHandler(IJobHandler jobHandler)
        {
            this._jobHandler = jobHandler;
        }

        public bool IsRunningOrHasQueue()
        {
            return _running || _taskQueue.Count > 0;
        }

        public void Clear()
        {
            while (_taskQueue.Count > 0)
            {
                _taskQueue.TryDequeue(out JobRunRequest jobRunRequest);
            }
        }

    }


}
