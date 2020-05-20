using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Foundation;
using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        private ConcurrentDictionary<int, HashSet<JobRunRequest>> ExecutingJobs = new ConcurrentDictionary<int, HashSet<JobRunRequest>>();

        public void RegistJobInfo(JobRunRequest jobInfo)
        {
            if (ExecutingJobs.ContainsKey(jobInfo.jobId))
            {
                ExecutingJobs[jobInfo.jobId].Add(jobInfo);
                return;
            }

            lock (ExecutingJobs)
            {
                if (ExecutingJobs.ContainsKey(jobInfo.jobId))
                {
                    ExecutingJobs[jobInfo.jobId].Add(jobInfo);
                }
                else
                {
                    ExecutingJobs.TryAdd(jobInfo.jobId, new HashSet<JobRunRequest> { jobInfo });
                }
            }
        }

        public void RemoveJobInfo(JobRunRequest jobInfo)
        {
            if (ExecutingJobs.TryGetValue(jobInfo.jobId, out HashSet<JobRunRequest> sets))
            {
                sets.Remove(jobInfo);
            }
        }

        public bool StopJob(int jobId, string stopReason)
        {
            var result = false;
            if (ExecutingJobs.TryGetValue(jobId, out HashSet<JobRunRequest> sets))
            {
                foreach (var item in sets)
                {
                    item.Stop = true;
                    item.StopReason = stopReason;
                    result = true;
                }
                sets.Clear();//其实这里不做清除，在执行任务时也会单个删除
            }
            return result;
        }

        public bool IsRunningOrHasQueue(int jobId)
        {
            if (ExecutingJobs.TryGetValue(jobId, out HashSet<JobRunRequest> sets))
            {
                return sets.Count > 0;
            }

            return false;
        }

        public int GetQueueCount(int jobId)
        {
            if (ExecutingJobs.TryGetValue(jobId, out HashSet<JobRunRequest> sets))
            {
                return sets.Count;
            }
            return 0;
        }
    }

    /// <summary>
    /// 任务执行器 暂时没用
    /// </summary>
    public class XxlJobExecutorBak
    {
        private IServiceProvider _serviceProvider;
        private ConcurrentDictionary<int, SingleJobExecutor> ExecuteJobs = new ConcurrentDictionary<int, SingleJobExecutor>();

        private IKeyLock JobExecutorLock = LocalKeyLock.Create();
        public XxlJobExecutorBak(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取单个任务执行器
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public SingleJobExecutor GetJobExecutor(int jobId)
        {
            if (ExecuteJobs.TryGetValue(jobId, out SingleJobExecutor singleJobExecutor))
            {
                return singleJobExecutor;
            }
            return null;
        }

        /// <summary>
        /// 注册单个任务执行器(移除之前的并停止)
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobHandler"></param>
        /// <param name="xxlJobExecutorService"></param>
        /// <returns></returns>
        public SingleJobExecutor RegistJobExecutor(int jobId, IJobHandler jobHandler, XxlJobExecutorService xxlJobExecutorService)
        {
            var lockKey = jobId.ToString();
            using (var lockObj = JobExecutorLock.Lock(lockKey))
            {
                var jobExecutor = new SingleJobExecutor(jobId, jobHandler, _serviceProvider);
                jobExecutor.Start();

                if (ExecuteJobs.TryRemove(jobId, out SingleJobExecutor oldJobExecutor))
                {
                    oldJobExecutor.Stop();
                }

                ExecuteJobs.TryAdd(jobId, jobExecutor);
                return jobExecutor;
            }
        }

        /// <summary>
        /// 移除任务(停止某个任务时使用)
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool RemoveJobExecutor(int jobId)
        {
            bool result = false;
            var lockKey = jobId.ToString();
            using (var lockObj = JobExecutorLock.Lock(lockKey))
            {
                result = ExecuteJobs.TryRemove(jobId, out SingleJobExecutor remove);
                if (result)
                {
                    remove.Stop();
                }
            }
            return result;
        }

    }
}
