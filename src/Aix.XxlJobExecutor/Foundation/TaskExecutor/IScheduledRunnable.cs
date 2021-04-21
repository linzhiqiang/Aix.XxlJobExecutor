using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.XxlJobExecutor.Foundation
{
    public interface IScheduledRunnable : IRunnable, IComparable<IScheduledRunnable>
    {
        long TimeStamp { get; }
    }

    public class ScheduledRunnable : IScheduledRunnable
    {
        public long TimeStamp { get; }

        IRunnable _action;

        public object state { get; set; }

        public ScheduledRunnable(IRunnable runnable, long timeStamp)
        {
            _action = runnable;
            this.state = runnable.state;
            TimeStamp = timeStamp;
        }

        public int CompareTo(IScheduledRunnable other)
        {
            return (int)(this.TimeStamp - other.TimeStamp);
        }

        public Task Run(object state)
        {
            return _action.Run(state);
        }

    }
}
