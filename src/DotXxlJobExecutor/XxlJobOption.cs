using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotXxlJobExecutor
{
    public class XxlJobOption
    {
        /// <summary>
        /// 访问token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// /执行器名称 对应xxljob执行器配置的appName
        /// </summary>
        public string ExecutorName { get; set; }

        /// <summary>
        /// xxljob调度中心根地址
        /// </summary>
        public string XxlJobAdminUrl { get; set; } = "http://localhost:8080/xxl-job-admin/";

        /// <summary>
        /// 当前执行器节点地址
        /// </summary>
        public string ExecutorUrl { get; set; } = "http://localhost:55860/api/xxljob/";

        public int TaskExecutorThreadCount { get; set; }

    }
}
