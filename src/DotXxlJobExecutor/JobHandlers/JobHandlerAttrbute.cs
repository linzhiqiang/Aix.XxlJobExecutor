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
        /// 执行器根据xxljob的JobHandler匹配
        /// </summary>
        public string Name { get; set; }

        public JobHandlerAttrbute()
        {
        }

        public static JobHandlerAttrbute GetJobHandlerAttrbute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(JobHandlerAttrbute), true);
            return attrs != null && attrs.Length > 0 ? attrs[0] as JobHandlerAttrbute : null;
        }

    }
}
