using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Aix.XxlJobExecutor.DTO
{
    /// <summary>
    /// 与xxljob调度中心交互响应
    /// </summary>
    [DataContract]
    public class ReturnT
    {
        public const int SUCCESS_CODE = 200;
        public const int FAIL_CODE = 500;
        //public const int INNER_ERROR_CODE = 500;

        //public static readonly ReturnT SUCCESS = new ReturnT(SUCCESS_CODE, null);
        //public static readonly ReturnT FAIL = new ReturnT(FAIL_CODE, null);
        //public static readonly ReturnT FAIL_TIMEOUT = new ReturnT(502, null);

        public ReturnT() {  }

        public ReturnT(int code, string msg)
        {
            Code = code;
            Msg = msg;
        }


        [DataMember(Name = "code")]
        public int Code { get; set; }
        [DataMember(Name = "msg")]
        public string Msg { get; set; }

        [DataMember(Name = "content")]
        public object Content { get; set; }

        public static ReturnT Failed(string msg)
        {
            return new ReturnT(FAIL_CODE, msg);
        }
        public static ReturnT Success(string msg="success")
        {
            return new ReturnT(SUCCESS_CODE, msg);
        }

    }
}
