using System;
using System.Collections.Generic;
using System.Text;

namespace DotXxlJobExecutor.Foundation
{
    public interface IKeyLock
    {
        IDisposable Lock(string key);
    }
}
