using Aix.XxlJobExecutor.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.JobHandlers
{
    [JobHandler(Name = "httpJobHandler")]
    public class HttpJobHandler : IJobHandler
    {
        private readonly ILogger<HttpJobHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly XxlJobOption _xxlJobOption;

        public static int SuccessCode = 0;

        public HttpJobHandler(ILogger<HttpJobHandler> logger, IHttpClientFactory httpClientFactory, XxlJobOption xxlJobOption)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _xxlJobOption = xxlJobOption;
        }
        public async Task<ReturnT> Execute(JobExecuteContext context)
        {
            var result = ReturnT.Success();
            try
            {
                var url = _xxlJobOption.HttpJobhandlerUrl;
                if (string.IsNullOrEmpty(url)) return ReturnT.Failed("httpJobHandler任务请配置url");

                JObject jobParameter = JObject.Parse(context.JobParameter);
                string path = GetPathAndRemovePath(jobParameter);
                if (string.IsNullOrEmpty(path)) return ReturnT.Failed("httpJobHandler任务请配置path");

                // jobParameter.Add("logId", context.LogId);

                var header = CreateHeader();
                var param = new { logId = context.LogId, jobParameter = jobParameter };
                var res = await _httpClientFactory.CreateClient().PostJsonAsync<HttpJobHandlerResponse>(StringUtils.AppendUrlPath(url, path), param, header);
                if (res == null || res.code != HttpJobHandler.SuccessCode)
                {
                    result = ReturnT.Failed(res?.msg ?? "执行http任务异常");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行httpJobHandler异常");
                result = ReturnT.Failed($"{ex.StackTrace},{ex.Message}");
            }

            return result;
        }

        private string GetPathAndRemovePath(JObject jobParameter)
        {
            string path = null;
            if (jobParameter.ContainsKey("_path"))
            {
                path = jobParameter["_path"].ToObject<string>();
                jobParameter.Remove("_path");
            }
            else if (jobParameter.ContainsKey("path"))
            {
                path = jobParameter["path"].ToObject<string>();
                jobParameter.Remove("path");
            }

            return path;
        }

        private IDictionary<string, string> CreateHeader()
        {
            IDictionary<string, string> header = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(_xxlJobOption.Token))
            {
                header.Add(XxlJobConstant.Token, _xxlJobOption.Token);
            }
            return header;
        }

    }
}
