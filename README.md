"# DotXxlJobExecutor" 

## �����ļ�
```
"XxlJobOption": {
    "ExecutorName": "demo",
    "Token": "demo",
    "XxlJobAdminUrl": "http://localhost:8080/xxl-job-admin/",
    "ExecutorUrl": "http://localhost:5000/api/xxljob/"
  }
 ```
  
## Startup���޸����£�

```
public void ConfigureServices(IServiceCollection services)
{
            #region xxljob

            var xxlJobOption = Configuration.GetSection("XxlJobOption").Get<XxlJobOption>();
            services.AddXxlJob(xxlJobOption);

            //��Ӿ�������
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

 ## �������

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
                _logger.LogInformation("firstJobHandlerִ����{a}, {b}", "1", 2);                                                          
                await Task.CompletedTask;
                //return ReturnT.Failed("����");
            }
            catch (BizException) //ҵ���쳣
            {

            }
            catch (Exception ex)
            {
                //ֻ��ϵͳ�쳣���ش��󣬱�������
                result = ReturnT.Failed($"{ex.StackTrace},{ex.Message}");
            }

            return result;
        }
    }
```