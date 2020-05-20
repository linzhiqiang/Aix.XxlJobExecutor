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
            app.MapEx("/api/xxljob/run", executor.HandleRun);
            app.MapEx("/api/xxljob/beat", executor.HandleBeat);
            app.MapEx("/api/xxljob/idlebeat", executor.HandleIdleBeat);
            app.MapEx("/api/xxljob/kill", executor.HandleKill);
            app.MapEx("/api/xxljob/log", executor.HandleLog);

            return app;
        }
    }


}
