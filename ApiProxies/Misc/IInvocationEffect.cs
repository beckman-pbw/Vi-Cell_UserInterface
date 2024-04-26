using System;

namespace ApiProxies.Misc
{
    /// <summary>
    /// This interface is an inheritance root for types that notify others upon being invoked.
    /// </summary>
    public interface IInvocationNotifier
    {
    }

    /// <summary>
    /// Describes a type that has an empty effect from a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    public interface IInvocationEffect : IInvocationNotifier
    {
        ApiEventArgs EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }

    /// <summary>
    /// Describes a type that holds the single data-type effect of a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    public interface IInvocationEffect<TEvArg1> : IInvocationNotifier, IInvocationResult<TEvArg1>
    {
        ApiEventArgs<TEvArg1> EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs<TEvArg1>> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }

    /// <summary>
    /// Describes a type that holds the 2 data-type effects of a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    public interface IInvocationEffect<TEvArg1, TEvArg2> : IInvocationNotifier, IInvocationResult<TEvArg1, TEvArg2>
    {
        ApiEventArgs<TEvArg1, TEvArg2> EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs<TEvArg1, TEvArg2>> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }

    /// <summary>
    /// Describes a type that holds the 3 data-type effects of a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    public interface IInvocationEffect<TEvArg1, TEvArg2, TEvArg3> : IInvocationNotifier,
        IInvocationResult<TEvArg1, TEvArg2, TEvArg3>
    {
        ApiEventArgs<TEvArg1, TEvArg2, TEvArg3> EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3>> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }

    /// <summary>
    /// Describes a type that holds the 4 data-type effects of a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    /// <typeparam name="TEvArg4"></typeparam>
    public interface IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4> : IInvocationNotifier,
        IInvocationResult<TEvArg1, TEvArg2, TEvArg3, TEvArg4>
    {
        ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4> EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4>> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }

    /// <summary>
    /// Describes a type that holds the 5 data-type effects of a method invocation and also notifies
    /// interested parties that said method was invoked.
    /// </summary>
    /// <typeparam name="TEvArg1"></typeparam>
    /// <typeparam name="TEvArg2"></typeparam>
    /// <typeparam name="TEvArg3"></typeparam>
    /// <typeparam name="TEvArg4"></typeparam>
    /// <typeparam name="TEvArg5"></typeparam>
    public interface IInvocationEffect<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> : IInvocationNotifier,
        IInvocationResult<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>
    {
        ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5> EventArgs { get; }

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ApiEventArgs<TEvArg1, TEvArg2, TEvArg3, TEvArg4, TEvArg5>> Invoked;

        /// <summary>
        /// Returns a value indicating whether the supplied object has already subscribed to the Invoked event.
        /// </summary>
        /// <param name="potentialSubscriber"></param>
        /// <returns></returns>
        bool IsSubscribed(object potentialSubscriber);
    }
}