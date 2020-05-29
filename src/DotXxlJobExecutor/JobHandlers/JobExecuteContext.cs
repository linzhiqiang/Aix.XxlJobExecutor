using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.JobHandlers
{
    public class JobExecuteContext
    {
        /// <summary>
        /// 本次调度日志ID
        /// </summary>
        public int LogId { get; set; }
        public string JobParameter { get; private set; }

        public JobExecuteContext(int logId, string jobParameter)
        {
            this.LogId = logId;
            this.JobParameter = jobParameter;
        }
    }
}
