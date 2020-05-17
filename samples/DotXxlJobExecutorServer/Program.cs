using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotXxlJobExecutorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configurationBuilder =>
            {
                //https://www.cnblogs.com/subendong/p/8834902.html
                //configurationBuilder.AddEnvironmentVariables(prefix: "Demo_");  ??
            })
           .ConfigureAppConfiguration((hostBulderContext, configurationBuilder) =>
           {
               //配置环境变量 ASPNETCORE _ENVIRONMENT: Development/Staging/Production(默认值) 
               //以下加载配置文件的方式，是系统的默认行为，如果改变配置文件路径 需要自己加载，否则没必要了
               //var environmentName = hostBulderContext.HostingEnvironment.EnvironmentName;
               //configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
               // configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);// 覆盖前面的相同内容
           })
            .ConfigureLogging((hostBulderContext, loggingBuilder) =>
            {
                //loggingBuilder.ClearProviders();
                //loggingBuilder.AddConsole();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://*:5000;https://*:5001");
                webBuilder.UseStartup<Startup>();
            });
    }
}
