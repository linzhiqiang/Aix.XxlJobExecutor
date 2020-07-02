using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Foundation
{
    public static class TaskExecutorExtensions
    {
        /// <summary>
        /// 注册任务执行器服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="taskExecutorThreadCount">默认Environment.ProcessorCount * 2</param>
        /// <returns></returns>
        public static IServiceCollection AddTaskExecutor(this IServiceCollection services, int taskExecutorThreadCount = 0)
        {
            services.AddSingleton<ITaskExecutor, MultithreadTaskExecutor>(provider =>
            {
                var logger = provider.GetService<ILogger<MultithreadTaskExecutor>>();
                var taskExecutor = new MultithreadTaskExecutor(taskExecutorThreadCount);
                taskExecutor.OnException += ex =>
                {
                    logger.LogError(ex, "任务执行出错");
                    return Task.CompletedTask;
                };
                taskExecutor.Start();
                return taskExecutor;
            });

            return services;
        }
    }
}
