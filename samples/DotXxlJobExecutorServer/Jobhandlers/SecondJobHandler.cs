﻿using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.JobHandlers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotXxlJobExecutorServer.Jobhandlers
{
    [JobHandlerAttrbute(Name = "SecondJobHandler", IsAsync = true)]
    public class SecondJobHandler : IJobHandler
    {
        ILogger<FirstJobHandler> _logger;

        public SecondJobHandler(ILogger<FirstJobHandler> logger)
        {
            _logger = logger;

        }
        public async Task<ReturnT> Execute(JobExecuteContext context)
        {
            var result = ReturnT.Success("success");
            try
            {
                //await Task.Delay(5000);
                _logger.LogInformation("SecondJobHandler执行了{0}, {1}", "1", 2);
                _logger.LogInformation("SecondJobHandler执行了{a}, {b}", "1", 2); //只占位符
                await Task.CompletedTask;
                //return ReturnT.Failed("错处啦");
            }
            catch (BizException) //业务异常
            {

            }
            catch (Exception ex)
            {
                //只有系统异常返回错误，便于重试
                result = ReturnT.Failed(ex.Message);
            }

            return result;


        }

        public class BizException : Exception
        {

        }
    }
}