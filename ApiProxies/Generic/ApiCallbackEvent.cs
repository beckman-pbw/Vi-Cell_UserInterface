using ApiProxies.Misc;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiProxies.Generic
{
    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    public abstract class ApiCallbackEvent : ApiCallbackEventBase<ApiEventArgs>, IInvocationEffect
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        protected override ApiEventArgs GetEventArgs()
        {
            var args = new ApiEventArgs() {EventType = EventType};
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }

        protected override void Reset()
        {
            EventArgs = null;
            InvocationExceptions.Clear();
        }
    }

    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    public abstract class ApiCallbackEvent<TEvArg1> : ApiCallbackEventBase<ApiEventArgs<TEvArg1>>, IInvocationEffect<TEvArg1>
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        public Tuple<TEvArg1> Results { get; protected set; }

        protected override void Reset()
        {
            EventArgs = null;
            // Safe to clear results now for next time
            Results = null;
            InvocationExceptions.Clear();
        }

        protected override ApiEventArgs<TEvArg1> GetEventArgs()
        {
            if (Results == null)
                return null;

            var args = new ApiEventArgs<TEvArg1>();
            args.EventType = EventType;
            args.Arg1 = Results.Item1;
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }
    }

    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    public abstract class ApiCallbackEvent<TEvArg1, TEvArg2> : ApiCallbackEventBase<ApiEventArgs<TEvArg1, TEvArg2>>, IInvocationEffect<TEvArg1, TEvArg2>
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        public Tuple<TEvArg1, TEvArg2> Results { get; protected set; }

        protected override void Reset()
        {
            EventArgs = null;
            // Safe to clear results now for next time
            Results = null;
            InvocationExceptions.Clear();
        }

        protected override ApiEventArgs<TEvArg1, TEvArg2> GetEventArgs()
        {
            if (Results == null)
                return null;

            var args = new ApiEventArgs<TEvArg1, TEvArg2>();
            args.EventType = EventType;
            args.Arg1 = Results.Item1;
            args.Arg2 = Results.Item2;
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }
    }

    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    public abstract class ApiCallbackEvent<TEvArg1, TEvArg2, TEvArg3> :
        ApiCallbackEventBase<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>>, IInvocationEffect<TEvArg1, TEvArg2, TEvArg3>
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        public Tuple<TEvArg1, TEvArg2, TEvArg3> Results { get; protected set; }

        protected override void Reset()
        {
            EventArgs = null;
            // Safe to clear results now for next time
            Results = null;
            InvocationExceptions.Clear();
        }

        protected override ApiEventArgs<TEvArg1, TEvArg2, TEvArg3> GetEventArgs()
        {
            if (Results == null)
                return null;

            var args = new ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>();
            args.EventType = EventType;
            args.Arg1 = Results.Item1;
            args.Arg2 = Results.Item2;
            args.Arg3 = Results.Item3;
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }
    }

    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    /// <typeparam name="TEvArg4"></typeparam>
    public abstract class ApiCallbackEvent<TEvArg1, TEvArg2, TEvArg3, TEvArg4> :
        ApiCallbackEventBase<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>>,
        IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4>
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        public Tuple<TEvArg1, TEvArg2, TEvArg3, TEvArg4> Results { get; protected set; }

        protected override void Reset()
        {
            EventArgs = null;
            // Safe to clear results now for next time
            Results = null;
            InvocationExceptions.Clear();
        }

        protected override ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4> GetEventArgs()
        {
            if (Results == null)
                return null;

            var args = new ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>();
            args.EventType = EventType;
            args.Arg1 = Results.Item1;
            args.Arg2 = Results.Item2;
            args.Arg3 = Results.Item3;
            args.Arg4 = Results.Item4;
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }
    }

    /// <summary>
    /// An application-facing abstract class that converts a stream of data from the API to application events.
    /// This class alleviates threading concerns for operations that cycle back and forth over the native-managed boundary
    /// through the use of a BlockingCollection as a data source for an IObservable that notifies IObservers on a new thread.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    /// <typeparam name="TEvArg4"></typeparam>
    /// <typeparam name="TEvArg5"></typeparam>
    public abstract class ApiCallbackEvent<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> :
        ApiCallbackEventBase<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>>,
        IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>
    {
        protected ApiCallbackEvent(string schedulerThreadName) : base(schedulerThreadName)
        {
        }

        public Tuple<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> Results { get; protected set; }

        protected override void Reset()
        {
            EventArgs = null;
            // Safe to clear results now for next time
            Results = null;
            InvocationExceptions.Clear();
        }

        protected override ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> GetEventArgs()
        {
            var args = new ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>();
            args.EventType = EventType;
            args.Arg1 = Results.Item1;
            args.Arg2 = Results.Item2;
            args.Arg3 = Results.Item3;
            args.Arg4 = Results.Item4;
            args.Arg5 = Results.Item5;
            if (InvocationExceptions.Any())
            {
                args.Exceptions = new List<Exception>(InvocationExceptions);
            }

            return args;
        }
    }
}