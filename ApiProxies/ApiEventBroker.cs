using ApiProxies.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using ApiProxies.Generic;
using ScoutUtilities.Common;

namespace ApiProxies
{
    /// <summary>
    /// This class decouples the API callback targets from the various subscribers
    /// that are interested in signals originating from the API.  API logic can now use and
    /// invoke its callbacks without concern for the lifecycle or memory validity of those callbacks.
    /// </summary>
    public sealed class ApiEventBroker : Singleton<ApiEventBroker>, IDisposable
    {
        private static readonly Dictionary<ApiEventType, IApiEvent> _apiCallbacks =
            new Dictionary<ApiEventType, IApiEvent>();

        private readonly object _disposeLock = new object();
        private bool _isDisposed = false; // To detect redundant calls

        public ApiEventBroker()
        {
            InitializeProxies();
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (!_isDisposed)
                {
                    foreach (IApiEvent ev in _apiCallbacks.Values)
                        ev.Dispose();

                    _isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe(ApiEventType evType, EventHandler<ApiEventArgs> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe(ApiEventType evType, EventHandler<ApiEventArgs> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        public void Subscribe<TEvArg1>(ApiEventType evType, Tuple<TEvArg1> types,
            EventHandler<ApiEventArgs<TEvArg1>> handler)
        {
            Subscribe(evType, handler);
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe<TEvArg1>(ApiEventType evType, EventHandler<ApiEventArgs<TEvArg1>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1>;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe<TEvArg1>(ApiEventType evType, EventHandler<ApiEventArgs<TEvArg1>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1>;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        public void Subscribe<TEvArg1, TEvArg2>(ApiEventType evType, Tuple<TEvArg1, TEvArg2> types,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2>> handler)
        {
            Subscribe(evType, handler);
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe<TEvArg1, TEvArg2>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2>;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe<TEvArg1, TEvArg2>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2>;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        public void Subscribe<TEvArg1, TEvArg2, TEvArg3>(ApiEventType evType, Tuple<TEvArg1, TEvArg2, TEvArg3> types,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>> handler)
        {
            Subscribe(evType, handler);
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe<TEvArg1, TEvArg2, TEvArg3>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3>;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe<TEvArg1, TEvArg2, TEvArg3>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3>;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        public void Subscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4>(ApiEventType evType,
            Tuple<TEvArg1, TEvArg2, TEvArg3, TEvArg4> types,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>> handler)
        {
            Subscribe(evType, handler);
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <typeparam name="TEvArg4">ApiEventArgs generic parameter 4</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4>;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <typeparam name="TEvArg4">ApiEventArgs generic parameter 4</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4>;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        public void Subscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>(ApiEventType evType,
            Tuple<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> types,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>> handler)
        {
            Subscribe(evType, handler);
        }

        /// <summary>
        /// Subscribe to the given event type using the given event handler.  This takes care of any object life-cycle
        /// concerns that exist in working with the API, and performs handler work on a Threadpool thread.
        /// Any number of handlers may subscribe to an event, but a specific handler will only be subscribed once.
        /// The handler will remain subscribed until a call to Unsubscribe removes it.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <typeparam name="TEvArg4">ApiEventArgs generic parameter 4</typeparam>
        /// <typeparam name="TEvArg5">ApiEventArgs generic parameter 5</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Subscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>;
            if (del != null && !del.IsSubscribed(handler.Target))
            {
                del.Invoked += handler;
            }
        }

        /// <summary>
        /// Unsubscribe from the given event type using the given event handler.  The event handler will no longer receive
        /// notifications for the given event type.
        /// </summary>
        /// <typeparam name="TEvArg1">ApiEventArgs generic parameter 1</typeparam>
        /// <typeparam name="TEvArg2">ApiEventArgs generic parameter 2</typeparam>
        /// <typeparam name="TEvArg3">ApiEventArgs generic parameter 3</typeparam>
        /// <typeparam name="TEvArg4">ApiEventArgs generic parameter 4</typeparam>
        /// <typeparam name="TEvArg5">ApiEventArgs generic parameter 5</typeparam>
        /// <param name="evType"></param>
        /// <param name="handler"></param>
        public void Unsubscribe<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>(ApiEventType evType,
            EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>> handler)
        {
            var del = _apiCallbacks[evType] as IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>;
            if (del != null)
            {
                del.Invoked -= handler;
            }
        }

        /// <summary>
        /// Gets an <see cref="ApiCallbackEventBase{TEvArgs}"/> instance for the given <see cref="ApiEventType"/>
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        internal IApiEvent GetProxyForApiEvent(ApiEventType eventType)
        {
            return _apiCallbacks[eventType];
        }

        /// <summary>
        /// Populate all of the concrete ApiCallbackEventBase.  Only types with a default constructor will be instantiated.
        /// </summary>
        private void InitializeProxies()
        {
            var assemblyTypes = new List<Type>(GetType().Assembly.GetTypes());

            var delegateCtors = from t in assemblyTypes
                let ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null)
                where ctor != null
                where typeof(IApiEvent).IsAssignableFrom(t)
                where t.IsAbstract == false
                select ctor;

            foreach (var ctor in delegateCtors)
            {
                try
                {
                    var delegateProxy = ctor.Invoke(new object[0]) as IApiEvent;
                    _apiCallbacks[delegateProxy.EventType] = delegateProxy;
                }
                catch (Exception ex)
                {
                    ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_PROXY_INITIALIZE");
                }
            }
        }
    }
}