using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor
{
    public static class JobHandlerExtensions
    {
        public static bool IsAsyncExecute(this IJobHandler jobHandler)
        {
            return JobHandlerAttrbute.IsAsyncExecute(jobHandler);
        }
    }
}
