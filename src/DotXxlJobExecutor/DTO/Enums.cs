using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.DTO
{
    /// <summary>
    ///  任务阻塞策略 SERIAL_EXECUTION=单机串行  DISCARD_LATER=丢弃后续调度  COVER_EARLY=覆盖之前调度 
    /// </summary>
    public enum ExecutorBlockStrategy
    {
        /// <summary>
        /// 单机串行
        /// </summary>
        SERIAL_EXECUTION = 1,

        /// <summary>
        /// 丢弃后续调度
        /// </summary>
        DISCARD_LATER = 2,

        /// <summary>
        /// 覆盖之前调度
        /// </summary>
        COVER_EARLY = 3
    }
}
