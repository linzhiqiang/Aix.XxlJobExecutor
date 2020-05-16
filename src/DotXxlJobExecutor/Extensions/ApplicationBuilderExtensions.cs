using DotXxlJobExecutor.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace DotXxlJobExecutor
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder MapEx(this IApplicationBuilder app, PathString pathMatch, Func<HttpContext, Task<object>> handle)
        {
            app.Map(pathMatch, appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var obj = await handle(context);
                    await context.Response.WriteAsync(JsonUtils.ToJson(obj));
                });
            });
            return app;
        }
    }
}
