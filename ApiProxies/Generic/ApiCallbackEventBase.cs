using ApiProxies.Misc;
using log4net;
using ScoutUtilities.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace ApiProxies.Generic
{
    public abstract class ApiCallbackEventBase<TEvArgs> : IApiEvent where TEvArgs : ApiEventArgs
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly object _callbackLock = new object();

        private readonly AsyncMessagePump<TEvArgs> _eventPump;

        // Create an IObservable that asynchronously pipes incoming events from a blocking collection
        // to an IObserver.  Observation is scheduled on a different thread from the data producing thread.
        protected ApiCallbackEventBase(string threadName) : this(NewThreadSchedulerAccessor.GetNormalPriorityScheduler(threadName))
        {
        }

        // Create an IObservable that pipes incoming events from a blocking collection
        // to an IObserver.  Observation is scheduled on the thread supplied by the given scheduler.
        protected ApiCallbackEventBase(IScheduler subscriptionScheduler)
        {
            _eventPump = new AsyncMessagePump<TEvArgs>(subscriptionScheduler, RaiseInvokedEvent);
        }

        public TEvArgs EventArgs { get; protected set; }

        protected List<Exception> InvocationExceptions { get; } = new List<Exception>();

        // Raised to indicate that the callback was invoked.  This carries all the relevant event data that has
        // already been marshaled to managed memory.
        public event EventHandler<TEvArgs> Invoked;

        public virtual void Dispose()
        {
            _eventPump.Dispose();
        }

        // Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        public bool IsSubscribed(object potentialSubscriber)
        {
            var delegates = Invoked?.GetInvocationList();

            if (delegates == null || potentialSubscriber == null)
            {
                return false;
            }

            return delegates.Any(d => potentialSubscriber.Equals(d.Target));
        }

        // Gets this classes API event type.
        public ApiEventType EventType { get; protected set; }

        // Marshals injected callback argument data (if there is any) into local properties in the managed memory space.
        // If an exception occurs in this method, then any data that arrived as arguments to the callback will not be
        // propagated along with the event.
        protected abstract void MarshalArgsToMembers();

        // Process argument data that was provided to the callback parameters, if there are any.
        // Any work that needs to be done before the API callback completes and the API reclaims its memory
        // should be done here.  This will occur on the same thread that initiated the callback in the API.
        protected virtual void ProcessUnmanagedData()
        {
            //empty base
        }

        protected abstract TEvArgs GetEventArgs();

        // Reset member variables that reference volatile native memory locations, or that will be reused upon
        // subsequent callback invocations.
        protected abstract void Reset();

        // First, marshals injected callback argument data (if there is any) into local properties in the managed memory
        // space.  Then processes argument data that was provided to the callback parameters, if there are any.
        // Finally, resets the object to make it ready for the next invocation.
        protected void OnCallback()
        {
            try
            {
                MarshalArgsToMembers();
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_CALL_BACK");
                InvocationExceptions.Add(ex);
            }

            try
            {
                ProcessUnmanagedData();
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_CALL_BACK");
                InvocationExceptions.Add(ex);
            }

            SubmitEventData();

            Reset();
        }

        // Submit the EventArgs created for a callback invocation to the blocking data source for
        // eventual consumption by the message pump.
        private void SubmitEventData()
        {
            EventArgs = GetEventArgs();
            // put the data into the data source for the IObservable
            _eventPump.Send(EventArgs);
        }

        private void RaiseInvokedEvent(TEvArgs args)
        {
            try
            {
                Invoked?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_RAISE_INVOKE_EVENT");
            }
        }
    }
}