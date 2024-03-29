﻿using Aix.XxlJobExecutor.DTO;
using Aix.XxlJobExecutor.Executor;
using Aix.XxlJobExecutor.Foundation;
using Aix.XxlJobExecutor.JobHandlers;
using Aix.XxlJobExecutor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor
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


        private JobExecutor _xxlJobExecutor;

        public XxlJobExecutorService(ILogger<XxlJobExecutorService> logger,
            XxlJobOption xxlJobOption,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            IJobHandlerManage jobHandlerManage,
            ITaskExecutor taskExecutor,
            JobExecutor xxlJobExecutor)
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
                        return ReturnT.Failed("终止任务[执行策略: DISCARD_LATER]");
                    }
                }
                else if (blockStrategy == ExecutorBlockStrategy.COVER_EARLY) //覆盖之前调度 负载之前积压的任务
                {
                    if (_xxlJobExecutor.IsRunningOrHasQueue(jobInfo.jobId))
                    {
                        _xxlJobExecutor.KillJob(jobInfo.jobId, "终止任务[执行策略：COVER_EARLY]"); //停止该jogid对应的所有积压的任务(已经在执行中的就停止不了)
                    }
                }

                await AsyncExecuteJob(jobInfo, jobHandler);
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
                _logger.LogError(ex, "xxljob触发任务错误");
            }
            return res;
        }

        /// <summary>
        /// 异步执行任务 把任务插入线程任务队列中排队执行
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="jobHandler"></param>
        /// <returns></returns>
        private async Task AsyncExecuteJob(JobRunRequest jobInfo, IJobHandler jobHandler)
        {
            Func<object, Task> action = async (state) =>
             {
                 if (jobInfo.JobStatus == JobStatus.Killed) //已终止的任务 就不要再运行了
                 {
                     _logger.LogInformation($"**************该任务已被终止 {jobInfo.jobId},{jobInfo.logId}********************");
                     return;
                 }

                 jobInfo.SetRunning();
                 ReturnT executeResult = null;
                 try
                 {
                     executeResult = await jobHandler.Execute(new JobExecuteContext(jobInfo.logId, jobInfo.executorParams));

                 }
                 catch (Exception ex)
                 {
                     executeResult = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
                     _logger.LogError(ex, "xxljob执行任务错误");
                 }

                 _xxlJobExecutor.RemoveJobInfo(jobInfo);
                 await CallBack(jobInfo.logId, executeResult); //这里保证一定要回调结果 不然就要重试了(配置了重试次数)，这里回调为失败结果也会重试(配置了重试次数)
             };

            _xxlJobExecutor.RegistJobInfo(jobInfo);
            //插入任务执行队列中 根据jobid路由到固定线程中 保证同一个jobid串行执行
            _taskExecutor.GetSingleThreadTaskExecutor(jobInfo.jobId).Execute(action, null);
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
                res = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
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
                var info = await context.Request.FromBody<JobIdleBeatRequest>();
                //_logger.LogInformation("--------------忙碌检测--------------");
                if (info != null)
                {
                    var isRunning = _xxlJobExecutor.IsRunningOrHasQueue(info.jobId);
                    if (isRunning)
                    {
                        return ReturnT.Failed("任务执行中");
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
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
                _logger.LogInformation($"--------------终止任务{info?.jobId}--------------");
                if (info != null)
                {
                    var success = _xxlJobExecutor.KillJob(info.jobId, "终止任务[调度中心主动停止任务]");
                    if (success)
                    {
                        return ReturnT.Success();
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
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
                res.Content = new
                {
                    fromLineNum = 0,        // 本次请求，日志开始行数
                    toLineNum = 100,        // 本次请求，日志结束行号
                    logContent = "暂无日志",     // 本次请求日志内容
                    isEnd = true            // 日志是否全部加载完
                };
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                res = ReturnT.Failed(ex.StackTrace + "————" + ex.Message);
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
        public async Task CallBackOld(int logId, ReturnT executeResult)
        {
            //todo: 回调任务改为多次重试的，保证回调成功，定时任务量一般不会很大，这里可以内存处理重试机制
            // var action = "";  //在action中判断重试次数，不到继续加入队列
            // _taskExecutor.GetSingleThreadTaskExecutor(logId).Execute(action);
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

        public async Task CallBack(int logId, ReturnT executeResult)
        {
            Func<object, Task> action = null;
            action = async (state) =>
            {
                var retryInfo = state as RetryCallbackDTO;
                try
                {
                    var calbackBody = new
                    {
                        code = executeResult.Code,
                        msg = executeResult.Msg,
                        logId = logId,
                        logDateTime = DateUtils.GetTimeStamp(),
                        executeResult = new { code = executeResult.Code, msg = executeResult.Msg }
                    };
                    var header = new Dictionary<string, string>();
                    header.Add(XxlJobConstant.Token, _xxlJobOption.Token);
                    var res = await _httpClientFactory.CreateClient().PostJsonAsync<ReturnT>(_xxlJobOption.XxlJobAdminUrl.AppendUrlPath("api/callback"), new object[] { calbackBody }, header);
                    if (res?.Code == ReturnT.SUCCESS_CODE)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"xxljob任务结果回调失败,logId:{logId},等待重试，第{retryInfo?.ErrorCount}次");
                }
                //处理重试
                if (retryInfo == null) return;
                if (retryInfo.ErrorCount > RetryCallbackDTO.MaxRetryCount) return;
                retryInfo.ErrorCount++;
                var delay = TimeSpan.FromSeconds(retryInfo.GetDelaySecond());
                _taskExecutor.GetSingleThreadTaskExecutor(logId).Schedule(action, retryInfo, delay);
            };

            _taskExecutor.GetSingleThreadTaskExecutor(logId).Execute(action, new RetryCallbackDTO());
            await Task.CompletedTask;
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

        #region  该执行器提供的httpjobhandler 回调完成任务

        public async Task<object> CompleteHttpJobHandler(HttpContext context)
        {
            var res = new HttpJobHandlerResponse() { code = HttpJobHandler.SuccessCode };
            try
            {
                var info = await context.Request.FromBody<HttpJobHandlerCompleteRequest>();
                var xxlJobCode = info.code == HttpJobHandler.SuccessCode ? ReturnT.SUCCESS_CODE : info.code;
                await this.CallBack(info.logId, new ReturnT(xxlJobCode, info.msg));
            }
            catch (Exception ex)
            {
                res.code = -1;
                res.msg = ex.StackTrace + "————" + ex.Message;
                _logger.LogError(ex, "httpjobhandler回调完成任务错误");
            }
            return res;
        }



        #endregion

    }

}
