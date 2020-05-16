using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotXxlJobExecutor
{
    public class XxlJobStartService : IHostedService
    {
        ILogger<XxlJobStartService> _logger;
        XxlJobExecutor _xxlJobExecutor;

        public XxlJobStartService(ILogger<XxlJobStartService> logger, XxlJobExecutor xxlJobExecutor)
        {
            _logger = logger;
            _xxlJobExecutor = xxlJobExecutor;

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            TimerRegistryExecutor(cancellationToken);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 定时注册执行器
        /// </summary>
        private void TimerRegistryExecutor(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                _logger.LogInformation($"开始定时注册执行器,间隔{XxlJobConstant.HeartbeatIntervalSecond}秒......");
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _xxlJobExecutor.RegistryExecutor();
                        await Task.Delay(TimeSpan.FromSeconds(XxlJobConstant.HeartbeatIntervalSecond), cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("程序关闭");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "xxljob注册执行器错误，重试中......");
                        await Task.Delay(TimeSpan.FromSeconds(XxlJobConstant.HeartbeatIntervalSecond), cancellationToken);
                    }
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _xxlJobExecutor.RegistryRemoveExecutor();
        }
    }
}
