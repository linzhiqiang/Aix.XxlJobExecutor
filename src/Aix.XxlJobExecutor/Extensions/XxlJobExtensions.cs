using Aix.XxlJobExecutor;
using Aix.XxlJobExecutor.Executor;
using Aix.XxlJobExecutor.Foundation;
using Aix.XxlJobExecutor.JobHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Aix.XxlJobExecutor
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
            SingleThreadTaskExecutor.MaxTaskCount = 100000;//10万条
            services
               .AddSingleton(option)
               .AddSingleton<XxlJobExecutorService>()
               .AddHttpClient()
               .AddSingleton<XxlJobMiddleware>()
               .AddHostedService<XxlJobStartService>()
               .AddSingleton<IJobHandlerManage, JobHandlerManage>()
               .AddSingleton<JobExecutor>()
               .AddSingleton<IJobHandler, HttpJobHandler>()
               .AddTaskExecutor(option.TaskExecutorThreadCount);

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

            //xxljob调度中心调用地址
            var executor = app.ApplicationServices.GetService<XxlJobExecutorService>();
            app.MapEx("/api/xxljob/run", executor.HandleRun);//触发任务
            app.MapEx("/api/xxljob/beat", executor.HandleBeat);//心跳检测
            app.MapEx("/api/xxljob/idleBeat", executor.HandleIdleBeat);//忙碌检测
            app.MapEx("/api/xxljob/kill", executor.HandleKill);//终止任务
            app.MapEx("/api/xxljob/log", executor.HandleLog);//查看执行日志

            //执行器提供api
            app.MapEx("/api/jobexecutor/complete", executor.CompleteHttpJobHandler);//外部回到完成任务(执行器提供的httpjobhandler)

            return app;
        }

    }


}
