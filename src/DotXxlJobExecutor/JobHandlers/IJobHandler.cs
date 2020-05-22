using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.JobHandlers
{
    /// <summary>
    /// 具体任务执行者接口
    /// </summary>
    public interface IJobHandler
    {
        Task<ReturnT> Execute(JobExecuteContext context);
    }

    /// <summary>
    /// 泛型类任务执行者 任务参数对应T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseJobHandler<T> : IJobHandler
    {
        public Task<ReturnT> Execute(JobExecuteContext context)
        {
            var obj = JsonUtils.FromJson<T>(context.JobParameter);
            return Execute(obj);
        }

        public abstract Task<ReturnT> Execute(T jobData);
    }



    /// <summary>
    /// 测试任务执行者
    /// </summary>
    [JobHandlerAttribute(Name = "defaultJobHandler")]
    public class DefaultJobHandler : IJobHandler
    {
        ILogger<DefaultJobHandler> _logger;

        public DefaultJobHandler(ILogger<DefaultJobHandler> logger)
        {
            _logger = logger;

        }
        public async Task<ReturnT> Execute(JobExecuteContext context)
        {
             await Task.Delay(500);
            _logger.LogInformation("defaultJobHandler执行了-------------------------");
            await Task.CompletedTask;
            return ReturnT.Success();

            // return ReturnT.Failed("失败原因");
        }
    }


}
