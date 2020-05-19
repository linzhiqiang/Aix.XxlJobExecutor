using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DotXxlJobExecutor.Foundation
{
    /// <summary>
    /// 本地锁
    /// </summary>
    public class LocalKeyLock : IKeyLock
    {
        public static IKeyLock Instance = new LocalKeyLock();

        private object SyncObj = new object();
        private Dictionary<string, LockUnit> Locks = new Dictionary<string, LockUnit>();

        public IDisposable Lock(string key)
        {
            return GetLock(key).Lock();
        }

        internal void Remove(LockUnit lockUnit)
        {
            lock (SyncObj)
            {
                lockUnit.LockCount--;
                if (lockUnit.LockCount <= 0)
                {
                    Locks.Remove(lockUnit.Key);
                }
            }
        }

        private LockUnit GetLock(string key)
        {
            LockUnit result = null;
            lock (SyncObj)
            {
                if (Locks.ContainsKey(key))
                {
                    result = Locks[key];
                }
                else
                {
                    result = new LockUnit(key, this);
                    Locks.Add(key, result);
                }

                result.LockCount++;
                return result;
            }

        }

    }

    internal class LockUnit : IDisposable
    {
        /// <summary>
        /// 等待线程数
        /// </summary>
        public int LockCount = 0;
        public string Key;

        private object SyncObj = new object();
        private LocalKeyLock LocalKeyLock;


        public LockUnit(string key, LocalKeyLock localKeyLock)
        {
            this.Key = key;
            this.LocalKeyLock = localKeyLock;
        }
        public LockUnit Lock()
        {
            //Interlocked.Increment(ref this.LockCount);
            Monitor.Enter(SyncObj);
            return this;
        }

        private void Realease()
        {
            // Interlocked.Decrement(ref this.LockCount);
            LocalKeyLock.Remove(this);
            Monitor.Exit(SyncObj);
        }

        public void Dispose()
        {
            this.Realease();
        }
    }
}
