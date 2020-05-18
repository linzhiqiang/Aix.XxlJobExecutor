using DotXxlJobExecutor.DTO;
using DotXxlJobExecutor.JobHandlers;
using DotXxlJobExecutor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using DotXxlJobExecutor.Foundation;

namespace DotXxlJobExecutor
{
    /// <summary>
    /// xxljob执行器服务
    /// </summary>
    public class XxlJobExecutorService
    {
        private ILogger<XxlJobExecutorService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private IServiceProvider _serviceProvider;
        private XxlJobOption _xxlJobOption;
        private IJobHandlerManage _jobHandlerManage;
        private ITaskExecutor _taskExecutor;

        private ConcurrentDictionary<string, IJobHandler> _jobHandlers = new ConcurrentDictionary<string, IJobHandler>();
        public XxlJobExecutorService(ILogger<XxlJobExecutorService> logger,
            XxlJobOption xxlJobOption,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            IJobHandlerManage jobHandlerManage,
            ITaskExecutor taskExecutor)
        {
            _logger = logger;
            _xxlJobOption = xxlJobOption;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _jobHandlerManage = jobHandlerManage;
            _taskExecutor = taskExecutor;
        }

        #region xxljob 触发    xxljob调度中心调用的接口

        /// <summary>
        /// 触发任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleRun(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                var jobInfo = await GetRequestFromBody<JobRunRequest>(context);
                _logger.LogInformation($"--------------触发任务{JsonUtils.ToJson(jobInfo)}--------------");
                //获取jobhandler并执行
                var jobHandler = _jobHandlerManage.GetJobHandler(jobInfo.executorHandler);
                if (jobHandler == null) throw new Exception($"没有对应的JobHandler,{jobInfo.executorHandler}");
                ExecutorBlockStrategy blockStrategy;
                Enum.TryParse<ExecutorBlockStrategy>(jobInfo.executorBlockStrategy, out blockStrategy);

                if (blockStrategy == ExecutorBlockStrategy.SERIAL_EXECUTION)
                {
                }
                else if (blockStrategy == ExecutorBlockStrategy.DISCARD_LATER) //如果该jobid正在执行或未执行（在队列中），直接返回，丢弃改请求
                {
                }
                else if (blockStrategy == ExecutorBlockStrategy.COVER_EARLY) //如果该jobid正在执行或未执行（在队列中），停止它，执行当前请求
                {

                }

                await AsyncExecuteJob(jobInfo, jobHandler);
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.Message);
                _logger.LogError(ex, "xxljob触发任务错误");
            }
            return res;
        }

        private async Task AsyncExecuteJob(JobRunRequest jobInfo, IJobHandler jobHandler)
        {
            Func<Task> action = async () =>
             {
                 var executeResult = await jobHandler.Execute(new JobExecuteContext(jobInfo.executorParams));
                 await CallBack(jobInfo.logId, executeResult);
             };

            _taskExecutor.Execute(action);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleBeat(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                _logger.LogInformation("--------------心跳检测--------------");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.Message);
                _logger.LogError(ex, "xxljob心跳检测错误");
            }
            return res;
        }

        /// <summary>
        /// 忙碌检测
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleIdleBeat(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                _logger.LogInformation("--------------忙碌检测--------------");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.Message);
                _logger.LogError(ex, "xxljob忙碌检测错误");
            }
            return res;
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleKill(HttpContext context)
        {
            var res = ReturnT.Failed("This feature is not supported");
            try
            {
                var info = await GetRequestFromBody<JobKillRequest>(context);
                _logger.LogInformation($"--------------终止任务{info?.jobId}--------------");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.Message);
                _logger.LogError(ex, "xxljob终止任务错误");
            }
            return res;
        }

        /// <summary>
        /// 查看执行日志
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleLog(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                var info = await GetRequestFromBody<JobGetLogRequest>(context);
                _logger.LogInformation($"--------------查看执行日志{info?.logId}--------------");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.Message);
                _logger.LogError(ex, "xxljob查看执行日志错误");
            }
            return res;
        }

        #endregion

        #region 调用xxljob  我们回调xxljob的接口

        /// <summary>
        /// 任务结果回调
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        public async Task CallBack(int logId, ReturnT executeResult)
        {
            try
            {
                var calbackBody = new
                {
                    logId = logId,
                    logDateTime = DateUtils.GetTimeStamp(),
                    executeResult = new { code = executeResult.Code, msg = executeResult.Msg }
                };
                var header = new Dictionary<string, string>();
                header.Add(XxlJobConstant.Token, _xxlJobOption.Token);
                var res = await _httpClientFactory.CreateClient().PostJsonAsync<ReturnT>(_xxlJobOption.XxlJobAdminUrl.AppendUrlPath("api/callback"), new object[] { calbackBody }, header);
                if (res.Code != ReturnT.SUCCESS_CODE)
                {
                    _logger.LogError($"xxljob任务结果回调失败,logId:{logId},{res.Code},{res.Msg}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "xxljob任务结果回调错误");
            }
        }

        /// <summary>
        /// 注册执行器
        /// </summary>
        /// <returns></returns>
        public async Task RegistryExecutor()
        {
            var registryBody = new
            {
                registryGroup = "EXECUTOR",//固定值
                registryKey = _xxlJobOption.ExecutorName,//执行器AppName
                registryValue = _xxlJobOption.ExecutorUrl//执行器地址
            };

            var header = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(_xxlJobOption.Token))
            {
                header.Add(XxlJobConstant.Token, _xxlJobOption.Token);
            }
            var res = await _httpClientFactory.CreateClient().PostJsonAsync<ReturnT>(_xxlJobOption.XxlJobAdminUrl.AppendUrlPath("api/registry"), registryBody, header);
            if (res.Code == ReturnT.SUCCESS_CODE)
            {
                //_logger.LogInformation("xxljob注册执行器成功");
            }
            else
            {
                _logger.LogError($"xxljob注册执行器失败,{res.Code},{res.Msg}");
            }
        }

        /// <summary>
        /// 移除执行器
        /// </summary>
        /// <returns></returns>
        public async Task RegistryRemoveExecutor()
        {
            try
            {
                var registryRemoveBody = new
                {
                    registryGroup = "EXECUTOR",
                    registryKey = _xxlJobOption.ExecutorName,
                    registryValue = _xxlJobOption.ExecutorUrl
                };

                var header = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(_xxlJobOption.Token))
                {
                    header.Add(XxlJobConstant.Token, _xxlJobOption.Token);
                }
                var res = await _httpClientFactory.CreateClient().PostJsonAsync<ReturnT>(_xxlJobOption.XxlJobAdminUrl.AppendUrlPath("api/registryRemove"), registryRemoveBody, header);
                if (res.Code == ReturnT.SUCCESS_CODE)
                {
                    _logger.LogInformation("xxljob移除执行器成功");
                }
                else
                {
                    _logger.LogError($"xxljob移除执行器失败,{res.Code},{res.Msg}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "xxljob移除执行器错误");
            }
        }

        #endregion

        private async Task<T> GetRequestFromBody<T>(HttpContext context)
        {
            //读取body并解析
            var reader = new StreamReader(context.Request.Body);
            var contentFromBody = await reader.ReadToEndAsync();
            return JsonUtils.FromJson<T>(contentFromBody);
        }
    }

}
