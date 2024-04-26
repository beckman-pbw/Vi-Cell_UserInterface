using ScoutUtilities.Common;
using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ScoutUtilities.Reactive
{
   
    public class AsyncMessagePump<TEvArgs> : Disposable 
    {
       
        private readonly BlockingCollection<TEvArgs> _callbackQueue = new BlockingCollection<TEvArgs>();

        private readonly IDisposable _eventQueueSubscription;

        public AsyncMessagePump(IScheduler subscriptionScheduler, Action<TEvArgs> onSubscribe)
        {
            _eventQueueSubscription = _callbackQueue.GetConsumingEnumerable().ToObservable()
                // The SubscribeOn thread is used to extract values from the source IEnumerable.
                // This creates a long-running subscription on the given thread because the
                // source IEnumerable is a blocking collection and therefore never ends.
                .SubscribeOn(subscriptionScheduler)
                .Subscribe(onSubscribe);
        }

      
        public void Send(TEvArgs evArgs)
        {
            if (IsDisposed)
                return;
            _callbackQueue.Add(evArgs);
        }

        protected override void DisposeManaged()
        {
            _callbackQueue?.CompleteAdding();
            _eventQueueSubscription?.Dispose();
            base.DisposeManaged();
        }
    }
}