using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.XxlJobExecutor.JobHandlers
{
    /// <summary>
    ///jobhandler标识， 执行器根据xxljob的JobHandler和具体的jobhandler匹配
    /// </summary>
    public class JobHandlerAttribute : Attribute
    {
        /// <summary>
        /// 执行器根据xxljob的JobHandler匹配
        /// </summary>
        public string Name { get; set; }

        public JobHandlerAttribute()
        {
        }

        public static JobHandlerAttribute GetJobHandlerAttrbute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(JobHandlerAttribute), true);
            return attrs != null && attrs.Length > 0 ? attrs[0] as JobHandlerAttribute : null;
        }

    }
}
