using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.XxlJobExecutor.JobHandlers
{
    public class HttpJobHandlerResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
    }

    public class HttpJobHandlerCompleteRequest
    {
        /// <summary>
        /// 日志id
        /// </summary>
        public int logId { get; set; }

        /// <summary>
        /// 0=成功 其他表示失败 
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 失败或成功的描述
        /// </summary>
        public string msg { get; set; }
    }
}
