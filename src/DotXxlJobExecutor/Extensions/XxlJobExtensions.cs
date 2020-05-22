using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.Executor;
using DotXxlJobExecutor.Foundation;
using DotXxlJobExecutor.JobHandlers;
using DotXxlJobExecutor.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotXxlJobExecutor
{
    public static class XxlJobExtensions
    {
        /// <summary>
        /// 注册相关服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static IServiceCollection AddXxlJob(this IServiceCollection services, XxlJobOption option)
        {
            services
               .AddSingleton(option)
               .AddSingleton<XxlJobExecutorService>()
               .AddHttpClient()
               .AddSingleton<XxlJobMiddleware>()
               .AddHostedService<XxlJobStartService>()
               .AddSingleton<IJobHandlerManage, JobHandlerManage>()
               .AddSingleton<XxlJobExecutor>()
               .AddTaskExecutor();

            return services;
        }

        /// <summary>
        /// 注册http请求插件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseXxlJob(this IApplicationBuilder app)
        {
            app.UseMiddleware<XxlJobMiddleware>();

            //不同url映射不同的处理方法
            var executor = app.ApplicationServices.GetService<XxlJobExecutorService>();
            app.MapEx("/api/xxljob/run", executor.HandleRun);//触发任务
            app.MapEx("/api/xxljob/beat", executor.HandleBeat);//心跳检测
            app.MapEx("/api/xxljob/idleBeat", executor.HandleIdleBeat);//忙碌检测
            app.MapEx("/api/xxljob/kill", executor.HandleKill);//终止任务
            app.MapEx("/api/xxljob/log", executor.HandleLog);//查看执行日志

            return app;
        }

        #region 

        /// <summary>
        /// 注册任务执行器服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddTaskExecutor(this IServiceCollection services)
        {
            services.AddSingleton<ITaskExecutor, MultithreadTaskExecutor>(provider =>
            {
                var option = provider.GetService<XxlJobOption>();
                var logger = provider.GetService<ILogger<MultithreadTaskExecutor>>();
                var taskExecutor = new MultithreadTaskExecutor(option.TaskExecutorThreadCount);
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

        #endregion
    }


}
