using Aix.XxlJobExecutor.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> FromBody<T>(this HttpRequest request)
        {
            //读取body并解析
            //request.EnableBuffering(); //引用组件 Microsoft.AspNetCore.Http
            var stream = request.Body;
            var reader = new StreamReader(stream);
            var contentFromBody = await reader.ReadToEndAsync();
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            return JsonUtils.FromJson<T>(contentFromBody);
        }
    }
}
