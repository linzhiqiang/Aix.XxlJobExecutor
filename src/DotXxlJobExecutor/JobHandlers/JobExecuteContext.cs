using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.JobHandlers
{
    public class JobExecuteContext
    {
        public string JobParameter { get; private set; }

        public JobExecuteContext(string jobParameter)
        {
            JobParameter = jobParameter;
        }
    }
}
