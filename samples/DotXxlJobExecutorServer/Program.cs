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
               //���û������� ASPNETCORE _ENVIRONMENT: Development/Staging/Production(Ĭ��ֵ) 
               //���¼��������ļ��ķ�ʽ����ϵͳ��Ĭ����Ϊ������ı������ļ�·�� ��Ҫ�Լ����أ�����û��Ҫ��
               //var environmentName = hostBulderContext.HostingEnvironment.EnvironmentName;
               //configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
               // configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);// ����ǰ�����ͬ����
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
