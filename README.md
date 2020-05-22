"# DotXxlJobExecutor" 

## 配置文件
```
"XxlJobOption": {
    "ExecutorName": "demo",
    "Token": "demo",
    "XxlJobAdminUrl": "http://localhost:8080/xxl-job-admin/",
    "ExecutorUrl": "http://localhost:5000/api/xxljob/"
  }
 ```
  
## Startup类修改如下：

```
public void ConfigureServices(IServiceCollection services)
{
            #region xxljob

            var xxlJobOption = Configuration.GetSection("XxlJobOption").Get<XxlJobOption>();
            services.AddXxlJob(xxlJobOption);

            //添加具体任务
            services.AddSingleton<IJobHandler, FirstJobHandler>();

            #endregion
}


public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
	...
	 #region xxljob

	  app.UseXxlJob();

	 #endregion
	...
}

```

 ## 添加任务

```
[JobHandlerAttrbute(Name = "firstJobHandler")]
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
                _logger.LogInformation("firstJobHandler执行了{a}, {b}", "1", 2);                                                          
                await Task.CompletedTask;
                //return ReturnT.Failed("错处啦");
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
```