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
using DotXxlJobExecutor.Executor;

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


        private XxlJobExecutor _xxlJobExecutor;

        public XxlJobExecutorService(ILogger<XxlJobExecutorService> logger,
            XxlJobOption xxlJobOption,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            IJobHandlerManage jobHandlerManage,
            ITaskExecutor taskExecutor,
            XxlJobExecutor xxlJobExecutor)
        {
            _logger = logger;
            _xxlJobOption = xxlJobOption;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _jobHandlerManage = jobHandlerManage;
            _taskExecutor = taskExecutor;

            _xxlJobExecutor = xxlJobExecutor;
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
                var jobInfo = await context.Request.FromBody<JobRunRequest>();
                //_logger.LogInformation($"--------------触发任务{JsonUtils.ToJson(jobInfo)}--------------");
                //获取jobhandler并执行
                var jobHandler = _jobHandlerManage.GetJobHandler(jobInfo.executorHandler);
                if (jobHandler == null) throw new Exception($"没有对应的JobHandler,{jobInfo.executorHandler}");

                //处理执行器策略 默认是串行执行
                Enum.TryParse(jobInfo.executorBlockStrategy, out ExecutorBlockStrategy blockStrategy);
                if (blockStrategy == ExecutorBlockStrategy.DISCARD_LATER) //如果有积压任务，丢弃当前任务
                {
                    if (_xxlJobExecutor.IsRunningOrHasQueue(jobInfo.jobId))
                    {
                        return ReturnT.Failed("block strategy effect: DISCARD_LATER");
                    }
                }
                else if (blockStrategy == ExecutorBlockStrategy.COVER_EARLY) //覆盖之前调度 负载之前积压的任务
                {
                    if (_xxlJobExecutor.IsRunningOrHasQueue(jobInfo.jobId))
                    {
                        _xxlJobExecutor.KillJob(jobInfo.jobId, "停止任务[执行策略：COVER_EARLY]"); //停止该jogid对应的所有积压的任务(已经在执行中的就停止不了)
                    }
                }
                _xxlJobExecutor.RegistJobInfo(jobInfo);
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
            //todo: 回调任务改为多次重试的，保证回调成功
            Func<Task> action = async () =>
            {
                _xxlJobExecutor.RemoveJobInfo(jobInfo);
                if (jobInfo.JobStatus == JobStatus.Killed) //已终止的任务 就不要再运行了
                {
                    _logger.LogInformation($"**************该任务已被关闭 {jobInfo.jobId},{jobInfo.logId}********************");
                    return;
                }
                jobInfo.SetRunning(); //设置为运行状态
                var executeResult = await jobHandler.Execute(new JobExecuteContext(jobInfo.executorParams));
                await CallBack(jobInfo.logId, executeResult); //这里保证一定要回调结果 不然就要重试了(配置了重试次数)，这里回调为失败结果也会重试(配置了重试次数)
            };

            //插入任务执行队列中 根据jobid路由到固定线程中 保证同一个jobid串行执行
            _taskExecutor.GetSingleThreadTaskExecutor(jobInfo.jobId).Execute(action);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 心跳检测  路由策略选择故障转移时触发
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleBeat(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                //_logger.LogInformation("--------------心跳检测--------------");
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
        /// 忙碌检测 路由策略选择忙碌转移时触发
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> HandleIdleBeat(HttpContext context)
        {
            var res = ReturnT.Success();
            try
            {
                //_logger.LogInformation("--------------忙碌检测--------------");
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
            var res = ReturnT.Failed("job has been executed or is executing");//This feature is not supported
            try
            {
                var info = await context.Request.FromBody<JobKillRequest>();
                _logger.LogInformation($"--------------停止任务{info?.jobId}--------------");
                if (info != null)
                {
                    var success = _xxlJobExecutor.KillJob(info.jobId, "停止任务[调度中心主动停止任务]");
                    if (success)
                    {
                        return ReturnT.Success();
                    }
                }
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
                var info = await context.Request.FromBody<JobGetLogRequest>();
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
        /// 任务结果回调  回调结果状态不是200 就会再次重试该任务(配置了重试次数).
        ///如果没有回调结果，在当前执行器服务关闭后且10分钟后，自动标识失败 也会重试的(配置了重试次数)
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

    }

}
