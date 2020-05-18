using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Foundation;
using DotXxlJobExecutor.JobHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Executor
{
    public interface ISingleJobExecutor
    {

    }

    public class SingleJobExecutor
    {
        private int _jobId;
        private IJobHandler _jobHandler;
        IBlockingQueue<JobRunRequest> _taskQueue = QueueFactory.Instance.CreateBlockingQueue<JobRunRequest>();
        volatile bool _isStart = false;
        public SingleJobExecutor(int jobId, IJobHandler handler)
        {
            _jobId = jobId;
            _jobHandler = handler;
        }

        public void Start()
        {
           
        }


    }


}
