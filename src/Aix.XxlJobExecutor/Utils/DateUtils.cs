using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.XxlJobExecutor.Utils
{
    internal static class DateUtils
    {
        public static long GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        public static long GetTimeStamp(DateTime now)
        {
            DateTime theDate = now;
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = theDate.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return (long)ts.TotalMilliseconds;
        }
    }
}
