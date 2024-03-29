﻿using Aix.XxlJobExecutor.DTO;
using Aix.XxlJobExecutor.Foundation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.Executor
{
    /// <summary>
    /// 任务执行器 
    /// </summary>
    public class JobExecutor
    {
        private IServiceProvider _serviceProvider;
        public JobExecutor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 待执行或执行中的任务
        /// </summary>
        private ConcurrentDictionary<int, ConcurrentHashSet<JobRunRequest>> ExecutingJobs = new ConcurrentDictionary<int, ConcurrentHashSet<JobRunRequest>>();

        /// <summary>
        /// 注册任务
        /// </summary>
        /// <param name="jobInfo"></param>
        public void RegistJobInfo(JobRunRequest jobInfo)
        {
            if (ExecutingJobs.ContainsKey(jobInfo.jobId))
            {
                ExecutingJobs[jobInfo.jobId].TryAdd(jobInfo);
                return;
            }

            lock (ExecutingJobs)
            {
                if (ExecutingJobs.ContainsKey(jobInfo.jobId))
                {
                    ExecutingJobs[jobInfo.jobId].TryAdd(jobInfo);
                }
                else
                {
                    var set = new ConcurrentHashSet<JobRunRequest>();
                    set.TryAdd(jobInfo);
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
            if (ExecutingJobs.TryGetValue(jobInfo.jobId, out ConcurrentHashSet<JobRunRequest> sets))
            {
                sets.TryRemove(jobInfo);
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
            if (ExecutingJobs.TryGetValue(jobId, out ConcurrentHashSet<JobRunRequest> sets))
            {
                foreach (var item in sets)
                {
                    if (item.SetKilled())
                    {
                        item.KilledReason = killedReason;
                        killedJobs.Add(item);
                    }
                }
                sets.Clear();//其实这里不做清除，在执行任务时也会单个删除
            }

            //异步回调停止的任务结果
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

        #region private 
        private int GetQueueItemCount(int jobId)
        {
            if (ExecutingJobs.TryGetValue(jobId, out ConcurrentHashSet<JobRunRequest> sets))
            {
                return sets.Count;
            }
            return 0;
        }

        #endregion
    }

}
