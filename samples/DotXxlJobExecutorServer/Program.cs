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
            System.Threading.ThreadPool.SetMinThreads(100, 100);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configurationBuilder =>
            {
            })
           .ConfigureAppConfiguration((hostBulderContext, configurationBuilder) =>
           {
           })
            .ConfigureLogging((hostBulderContext, loggingBuilder) =>
            {
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //webBuilder.UseUrls("http://*:5000;https://*:5001"); //ÅäÖÃappsettings.jsonÖĞµÄurls 
                webBuilder.UseStartup<Startup>();
            });

    }
}
