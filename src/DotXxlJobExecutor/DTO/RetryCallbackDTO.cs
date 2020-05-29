using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.DTO
{
   public class RetryCallbackDTO
    {
        public static int MaxRetryCount = 5;
        public static int[] DefaultRetryStrategy = new int[] { 5, 10, 30, 60, 60, 2 * 60, 2 * 60, 5 * 60, 5 * 60 };


        public int ErrorCount { get; set; }

        public  int GetDelaySecond()
        {
            var retryStrategy = DefaultRetryStrategy;
            var errorCount = ErrorCount;
            if (errorCount < retryStrategy.Length)
            {
                return retryStrategy[errorCount];
            }
            return retryStrategy[retryStrategy.Length - 1];
        }
    }
}
