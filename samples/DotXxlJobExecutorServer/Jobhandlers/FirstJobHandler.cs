using Aix.XxlJobExecutor.DTO;
using Aix.XxlJobExecutor.JobHandlers;
using DotXxlJobExecutorServer.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DotXxlJobExecutorServer.Jobhandlers
{
    [JobHandlerAttribute(Name = "firstJobHandler")]
    public class FirstJobHandler : IJobHandler
    {
        ILogger<FirstJobHandler> _logger;

        public FirstJobHandler(ILogger<FirstJobHandler> logger)
        {
            _logger = logger;

        }
        public async Task<ReturnT> Execute(JobExecuteContext context)
        {
            var result = ReturnT.Success();
            try
            {
               // await Task.Delay(30000);
                //这里执行业务逻辑
                //_logger.LogInformation("firstJobHandler执行了{0}, {1}", "1", 2);
                _logger.LogInformation("firstJobHandler执行了{a}, {b}", "1", 2); //只占位符
                //throw new Exception(" 系统异常了，要重试了吧");
                await Task.CompletedTask;
            }
            catch (BizException) //业务异常
            {

            }
            catch (Exception ex)
            {
                //只有系统异常返回错误，便于重试
                result = ReturnT.Failed($"{ex.StackTrace},{ex.Message}");
            }

            return result;


        }

        
    }
}
