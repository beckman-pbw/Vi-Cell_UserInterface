using System;
using System.Reactive.Concurrency;
using System.Threading;

namespace ScoutUtilities.Reactive
{
    public static class NewThreadSchedulerAccessor
    {
        public static IScheduler GetNormalPriorityScheduler(string name = null)
        {
            Func<ThreadStart, Thread> threadFactory;
            if (string.IsNullOrEmpty(name))
            {
                threadFactory = threadStart => new Thread(threadStart) {Priority = ThreadPriority.Normal};
            }
            else
            {
                threadFactory = threadStart => new Thread(threadStart) {Priority = ThreadPriority.Normal, Name = name};
            }

            return new NewThreadScheduler(threadFactory);
        }

    }
}