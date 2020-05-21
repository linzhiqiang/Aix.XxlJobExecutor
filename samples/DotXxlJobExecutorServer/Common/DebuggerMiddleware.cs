using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutorServer.Common
{
    public class DebuggerMiddleware : IMiddleware
    {
        ILogger<DebuggerMiddleware> _logger;
        public DebuggerMiddleware(ILogger<DebuggerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string body = "";
            var request = context.Request;
            if (string.Compare(request.Method, HttpMethods.Get, true) == 0)
            {
                //body = request.QueryString.Value;//?a=1&b=2
                IDictionary<string, object> datas = new Dictionary<string, object>();
                foreach (var item in  request.Query)
                {
                    datas.Add(item.Key, item.Value.ToString());
                }
                body = JsonConvert.SerializeObject(datas);

            }
            else if (string.Compare(request.Method, HttpMethods.Post, true) == 0)
            {
                if (string.Compare(request.ContentType, "application/x-www-form-urlencoded", true) == 0)
                {
                    IDictionary<string, object> datas = new Dictionary<string, object>();
                    foreach (var item in await request.ReadFormAsync())
                    {
                        datas.Add(item.Key, item.Value.ToString());
                    }
                    body = JsonConvert.SerializeObject(datas);
                }
                else 
                {
                    request.EnableBuffering();
                    var reader = new StreamReader(request.Body, Encoding.UTF8);
                    body = await reader.ReadToEndAsync();
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
            }
            _logger.LogInformation($"--------------请求url:[{request.Method}]{context.Request.Path.Value},body：{body}--------------");
            await next(context);
        }
    }
}
