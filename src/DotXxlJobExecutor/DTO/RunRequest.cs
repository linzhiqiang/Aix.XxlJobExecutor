using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.DTO
{
    /// <summary>
    /// 执行器请求
    /// </summary>
    public class RunRequest
    {
        public int jobId { get; set; }
        public string executorHandler { get; set; }

        public string executorParams { get; set; }

        public string executorBlockStrategy { get; set; }

        public int executorTimeout { get; set; }

        public int logId { get; set; }

        public long logDateTime { get; set; }
    }

    //{"jobId":2,"executorHandler":"no","executorParams":"1","executorBlockStrategy":"SERIAL_EXECUTION","executorTimeout":0,"logId":87,"logDateTime":1589447542244,
    //"glueType":"BEAN","glueSource":"","glueUpdatetime":1589445790000,"broadcastIndex":0,"broadcastTotal":1}
}
