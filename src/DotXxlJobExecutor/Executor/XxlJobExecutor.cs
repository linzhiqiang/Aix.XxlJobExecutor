using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Foundation;
using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace DotXxlJobExecutor.Executor
{
    /// <summary>
    /// 任务执行器 
    /// </summary>
    public class XxlJobExecutor
    {
        private IServiceProvider _serviceProvider;
        public XxlJobExecutor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 待执行或执行中的任务
        /// </summary>
        private ConcurrentDictionary<int, ConcurrentDictionary<JobRunRequest, bool>> ExecutingJobs = new ConcurrentDictionary<int, ConcurrentDictionary<JobRunRequest, bool>>();

        /// <summary>
        /// 注册任务
        /// </summary>
        /// <param name="jobInfo"></param>
        public void RegistJobInfo(JobRunRequest jobInfo)
        {
            if (ExecutingJobs.ContainsKey(jobInfo.jobId))
            {
                ExecutingJobs[jobInfo.jobId].TryAdd(jobInfo, true);
                return;
            }

            lock (ExecutingJobs)
            {
                if (ExecutingJobs.ContainsKey(jobInfo.jobId))
                {
                    ExecutingJobs[jobInfo.jobId].TryAdd(jobInfo, true);
                }
                else
                {
                    var set = new ConcurrentDictionary<JobRunRequest, bool>();
                    set.TryAdd(jobInfo, true);
                    ExecutingJobs.TryAdd(jobInfo.jobId, set);
                }
            }
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="jobInfo"></param>
        public void RemoveJobInfo(JobRunRequest jobInfo)
        {
            if (ExecutingJobs.TryGetValue(jobInfo.jobId, out ConcurrentDictionary<JobRunRequest, bool> sets))
            {
                sets.TryRemove(jobInfo, out _);
            }
        }

        /// <summary>
        /// 停止队列中待执行的任务 这里采用先打标记，执行时根据该标记过滤
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="killedReason"></param>
        /// <returns></returns>
        public bool KillJob(int jobId, string killedReason)
        {
            var killedJobs = new List<JobRunRequest>();
            if (ExecutingJobs.TryGetValue(jobId, out ConcurrentDictionary<JobRunRequest, bool> sets))
            {
                foreach (var keyValue in sets)
                {
                    var item = keyValue.Key;
                    if (item.SetKilled())
                    {
                        item.KilledReason = killedReason;
                        killedJobs.Add(item);
                    }
                }
                sets.Clear();//其实这里不做清除，在执行任务时也会单个删除
            }

            //回调结果
            Task.Run(async () =>
            {
                foreach (var item in killedJobs)
                {
                    await _serviceProvider.GetService<XxlJobExecutorService>().CallBack(item.logId, ReturnT.Failed(killedReason));
                }
            });


            return killedJobs.Count > 0;
        }

        /// <summary>
        /// 判断该任务是否在执行中或在队列中待执行
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool IsRunningOrHasQueue(int jobId)
        {
            return GetQueueItemCount(jobId) > 0;
        }

        private int GetQueueItemCount(int jobId)
        {
            if (ExecutingJobs.TryGetValue(jobId, out ConcurrentDictionary<JobRunRequest, bool> sets))
            {
                return sets.Count;
            }
            return 0;
        }
    }

}
