using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Executor
{
    public class XxlJobExecutor
    {
        private ConcurrentDictionary<int, bool> WaitExecuteJobs = new ConcurrentDictionary<int, bool>();

        public XxlJobExecutor()
        {

        }

        public async Task ExecuteJob(JobRunRequest jobInfo, IJobHandler jobHandler)
        {
            ExecutorBlockStrategy blockStrategy;
            Enum.TryParse<ExecutorBlockStrategy>(jobInfo.executorBlockStrategy, out blockStrategy);

            var jobExecuteId = Guid.NewGuid().ToString();
            if (blockStrategy == ExecutorBlockStrategy.SERIAL_EXECUTION)
            {
            }
            else if (blockStrategy == ExecutorBlockStrategy.DISCARD_LATER) //如果该jobid正在执行或未执行（在队列中），直接返回，丢弃改请求
            {

            }
            else if (blockStrategy == ExecutorBlockStrategy.COVER_EARLY) //如果该jobid正在执行或未执行（在队列中），停止它，执行当前请求
            {

            }

            await Task.CompletedTask;
        }

      
    }
}
