using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotXxlJobExecutorServer.Common
{
    /// <summary>
    /// 业务异常
    /// </summary>
    public class BizException : Exception
    {
        public int Code { get; set; }

        public BizException(int code, string message) : base(message)
        {
            this.Code = code;
        }
    }
}
