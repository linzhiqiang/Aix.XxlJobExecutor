using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotXxlJobExecutor.Foundation
{
    public interface IRunnable
    {
        object state { get; }
        Task Run(object state=null);
    }

    public class TaskRunnable : IRunnable
    {
        Func<object,Task> _action;
        public TaskRunnable(Func<object,Task> action,object state)
        {
            _action = action;
            this.state = state;

        }

        public object state { get; set; }

        public Task Run(object state=null)
        {
            return _action(state);
        }
    }
}
