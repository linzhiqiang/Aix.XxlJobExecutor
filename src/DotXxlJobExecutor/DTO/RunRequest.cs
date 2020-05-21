using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.DTO
{
    /// <summary>
    /// 执行器执行任务请求 
    /// </summary>
    public class JobRunRequest
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int jobId { get; set; }

        /// <summary>
        /// 任务标识 JobHandler
        /// </summary>
        public string executorHandler { get; set; }

        /// <summary>
        /// 任务参数
        /// </summary>
        public string executorParams { get; set; }

        /// <summary>
        /// 任务阻塞策略 SERIAL_EXECUTION=单机串行  DISCARD_LATER=丢弃后续调度  COVER_EARLY=覆盖之前调度 
        /// </summary>
        public string executorBlockStrategy { get; set; }

        /// <summary>
        /// 任务超时时间，单位秒，大于零时生效
        /// </summary>
        public int executorTimeout { get; set; }

        /// <summary>
        /// 本次调度日志ID
        /// </summary>
        public int logId { get; set; }

        /// <summary>
        /// 本次调度日志时间
        /// </summary>
        public long logDateTime { get; set; }

        /// <summary>
        /// 任务模式
        /// </summary>
        public string glueType { get; set; }

        /// <summary>
        /// GLUE脚本代码
        /// </summary>
        public string glueSource { get; set; }

        /// <summary>
        /// GLUE脚本更新时间，用于判定脚本是否变更以及是否需要刷新
        /// </summary>
        public long glueUpdatetime { get; set; }

        /// <summary>
        /// 分片参数：当前分片
        /// </summary>
        public int broadcastIndex { get; set; }

        /// <summary>
        /// 分片参数：总分片
        /// </summary>
        public int broadcastTotal { get; set; }

        #region 内部属性

        //public bool Running { get; set; }

        /// <summary>
        /// 当前任务是否已停止
        /// </summary>
        public JobStatus JobStatus { get; set; } = JobStatus.Init;

        /// <summary>
        /// 停止原因
        /// </summary>
        public string KilledReason { get; set; }

        public bool SetKilled()
        {
            lock (this)
            {
                if (this.JobStatus == JobStatus.Init)
                {
                    this.JobStatus = JobStatus.Killed;
                    return true;
                }
            }
            return false;
        }

        public bool SetRunning()
        {
            lock (this)
            {
                if (this.JobStatus == JobStatus.Init)
                {
                    this.JobStatus = JobStatus.Running;
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

    //{"jobId":2,"executorHandler":"no","executorParams":"1","executorBlockStrategy":"SERIAL_EXECUTION","executorTimeout":0,"logId":87,"logDateTime":1589447542244,
    //"glueType":"BEAN","glueSource":"","glueUpdatetime":1589445790000,"broadcastIndex":0,"broadcastTotal":1}

    /// <summary>
    /// 终止任务请求
    /// </summary>
    public class JobKillRequest
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int jobId { get; set; }
    }

    /// <summary>
    /// 查看日志请求
    /// </summary>
    public class JobGetLogRequest
    {
        public long logDateTim { get; set; }

        public int logId { get; set; }

        public int fromLineNum { get; set; }
    }
    //{"logDateTim":1589791528000,"logId":801,"fromLineNum":1}
}
