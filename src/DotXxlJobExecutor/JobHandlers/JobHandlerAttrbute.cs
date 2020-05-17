using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.JobHandlers
{
    /// <summary>
    ///jobhandler标识， 执行器根据xxljob的JobHandler和具体的jobhandler匹配
    /// </summary>
    public class JobHandlerAttrbute : Attribute
    {
        /// <summary>
        /// 默认异步执行任务
        /// </summary>
        private const bool DefaultIsAsync = true;
        public string Name { get; set; }

        public JobHandlerAttrbute()
        {
            // IsAsync = DefaultIsAsync;
        }

        public static JobHandlerAttrbute GetJobHandlerAttrbute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(JobHandlerAttrbute), true);
            return attrs != null && attrs.Length > 0 ? attrs[0] as JobHandlerAttrbute : null;
        }

    }
}
