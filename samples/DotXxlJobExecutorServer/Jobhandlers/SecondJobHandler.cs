using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.JobHandlers;
using DotXxlJobExecutorServer.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotXxlJobExecutorServer.Jobhandlers
{
    [JobHandlerAttribute(Name = "SecondJobHandler")]
    public class SecondJobHandler : IJobHandler
    {
        ILogger<SecondJobHandler> _logger;

        public SecondJobHandler(ILogger<SecondJobHandler> logger)
        {
            _logger = logger;
        }
        public async Task<ReturnT> Execute(JobExecuteContext context)
        {
            var result = ReturnT.Success();
            try
            {
                //await Task.Delay(30000);
                //这里执行业务逻辑
                _logger.LogInformation("SecondJobHandler执行了-------------------------");
                await Task.CompletedTask;
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

    }
}
