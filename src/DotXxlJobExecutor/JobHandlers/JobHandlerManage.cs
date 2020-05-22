using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DotXxlJobExecutor.JobHandlers
{
    /// <summary>
    /// jobhandler管理
    /// </summary>
    public interface IJobHandlerManage
    {
        /// <summary>
        /// 注册任务
        /// </summary>
        /// <param name="jobhandlerName"></param>
        /// <param name="jobHandler"></param>
        /// <returns></returns>
        IJobHandlerManage AddJobHandler(string jobhandlerName, IJobHandler jobHandler);

        /// <summary>
        /// 根据调度中心配置jobhandler获取具体任务执行者
        /// </summary>
        /// <param name="jobhandlerName"></param>
        /// <returns></returns>
        IJobHandler GetJobHandler(string jobhandlerName);
    }


    public class JobHandlerManage : IJobHandlerManage
    {
        private IServiceProvider _serviceProvider;
        private ConcurrentDictionary<string, IJobHandler> _jobHandlers = new ConcurrentDictionary<string, IJobHandler>();
        public JobHandlerManage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Init();
        }

        private void Init()
        {
            //注册任务
            var jobHandlers = _serviceProvider.GetServices<IJobHandler>();
            foreach (var item in jobHandlers)
            {
                var attr = JobHandlerAttribute.GetJobHandlerAttrbute(item.GetType());
                // if (attr == null) throw new Exception($"请配置JobHandlerAttrbute,{item.GetType().FullName}");
                var jobHandlerName = attr != null ? attr.Name : item.GetType().Name;
                this.AddJobHandler(jobHandlerName, item);
            }
        }

        public IJobHandlerManage AddJobHandler(string jobhandlerName, IJobHandler jobHandler)
        {
            var key = jobhandlerName.ToLower();
            if (_jobHandlers.ContainsKey(key))
            {
                throw new Exception($"JobHandlerAttrbute配置重复,{jobHandler.GetType().FullName}");
            }
            _jobHandlers.TryAdd(key, jobHandler);
            return this;
        }

        public IJobHandler GetJobHandler(string jobhandlerName)
        {
            if (_jobHandlers.TryGetValue(jobhandlerName.ToLower(), out IJobHandler jobHandler))
            {
                return jobHandler;
            }

            return jobHandler;
        }
    }


}
