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
               .AddSingleton<ITaskExecutor, MultithreadTaskGroup>(provider =>
               {
                   var taskExecutor = new MultithreadTaskGroup(option.TaskExecutorThreadCount);
                   taskExecutor.Start();
                   return taskExecutor;
               });

            return services;
        }
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
    }


}
