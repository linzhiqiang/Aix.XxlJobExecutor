using Aix.XxlJobExecutor;
using Aix.XxlJobExecutor.JobHandlers;
using Aix.XxlJobExecutor;
using DotXxlJobExecutorServer.Common;
using DotXxlJobExecutorServer.Jobhandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotXxlJobExecutorServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// 配置依赖注入
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<DebuggerMiddleware>();
            //services.Configure<XxlJobOption>(Configuration.GetSection("XxlJobOption"));  //这样接收 注入 IOptions<XxlJobOption> options
            /*
            var xxlJobOption = new XxlJobOption
            {
                ExecutorName = "demo",
                Token = "demo",
                XxlJobAdminUrl = "http://localhost:8080/xxl-job-admin/",
                ExecutorUrl = "http://localhost:5000/api/xxljob/"
            };*/
            #region xxljob

            var xxlJobOption = Configuration.GetSection("XxlJobOption").Get<XxlJobOption>();
            services.AddXxlJob(xxlJobOption);

            //添加具体任务
            services.AddSingleton<IJobHandler, DefaultJobHandler>();
            services.AddSingleton<IJobHandler, FirstJobHandler>();
            services.AddSingleton<IJobHandler, SecondJobHandler>();

            #endregion
        }

        /// <summary>
        /// 配置http管道
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();
            if (env.IsDevelopment())
            {
                //调试中间件
                app.UseMiddleware<DebuggerMiddleware>();
            }

            #region xxljob

            app.UseXxlJob();

            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapPost("/", context =>
                //{
                //    return Task.CompletedTask;
                //});
            });
        }
    }
}
